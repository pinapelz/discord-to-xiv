using System;
using Fleck;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Threading;
using System.Text.Json;
using System.Text.Json.Serialization;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using DiscordToXIV.Windows;



namespace DiscordToXIV;


public class Message
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }
    [JsonPropertyName("author")]
    public string? Author { get; init; }

    [JsonPropertyName("author_name")]
    public string? AuthorName { get; init; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; init; }

    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("time")]
    public string? Time { get; init; }

    [JsonPropertyName("channel")]
    public string? Channel { get; init; }
    
    [JsonPropertyName("sticker_id")]
    public string? StickerId { get; init; }
    
    [JsonPropertyName("sticker_name")]
    public string? StickerName { get; init; }

    [JsonIgnore]
    public string? ChannelName { get; set; }
}


public sealed class Plugin : IDalamudPlugin
{
    private int[] nameColor =
    {
        45, 517, 704, 708, 52, 61
    };
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;

    private const string CommandName = "/pdiscordtoxiv";
    private const int DefaultPort = 8765;

    private WebSocketServer webSocketServer;
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly List<IWebSocketConnection> connectedClients;
    private readonly HashSet<string> recentMessages = new HashSet<string>();
    private readonly Queue<string> messageQueue = new Queue<string>();
    private const int MaxRecentMessages = 100;
    private readonly object messageLock = new object();
    public Configuration Configuration { get; init; }
    private ConfigWindow ConfigWindow { get; init; }
    public readonly WindowSystem WindowSystem = new("DiscordToXIV");
    

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);
        cancellationTokenSource = new CancellationTokenSource();
        connectedClients = new List<IWebSocketConnection>();

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Start WebSocket server. Usage: /pdiscordtoxiv [port]"
        });
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        ChatGui.Print("Plugin starting...");
    }

    public void Dispose()
    {
        StopWebSocketServer();
        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        var port = DefaultPort;
        if (!string.IsNullOrEmpty(args))
        {
            if(args == "stop")
            {
                StopWebSocketServer();
                ChatGui.Print("WebSocket server stop requested");
                return;
            }
            if (!int.TryParse(args, out port) || port <= 0 || port > 65535)
            {
                ChatGui.PrintError($"Invalid port number: {args}. Using default port {DefaultPort}.");
                port = DefaultPort;
            }
        }

        StartWebSocketServer(port);
        ChatGui.Print($"WebSocket server started on port {port}");
    }

private void StartWebSocketServer(int port)
{
    StopWebSocketServer();

    webSocketServer = new WebSocketServer($"ws://0.0.0.0:{port}");

    webSocketServer.Start(socket =>
    {
        socket.OnOpen = () =>
        {
            PluginLog.Information("WebSocket connection opened.");
            connectedClients.Add(socket);
        };

        socket.OnClose = () =>
        {
            PluginLog.Information("WebSocket connection closed.");
            connectedClients.Remove(socket);
        };

        socket.OnMessage = message =>
        {
            try
            {
                var receivedMessage = JsonSerializer.Deserialize<Message>(message);

                if (receivedMessage == null || string.IsNullOrEmpty(receivedMessage.Id))
                {
                    PluginLog.Error("Received message without an ID.");
                    return;
                }

                lock (messageLock)
                {
                    if (recentMessages.Contains(receivedMessage.Id))
                    {
                        PluginLog.Information("Duplicate message detected by ID, skipping.");
                        return;
                    }

                    recentMessages.Add(receivedMessage.Id);
                    messageQueue.Enqueue(receivedMessage.Id);

                    if (messageQueue.Count > MaxRecentMessages)
                    {
                        string oldMessageId = messageQueue.Dequeue();
                        recentMessages.Remove(oldMessageId);
                    }
                }
                var seString = new SeString(new List<Payload>());
                var name = receivedMessage.Author;
                ushort nameColor = (ushort)GetNameColor(name);
                if (receivedMessage.Nickname != null)
                {
                    if (!Configuration.HideUsernameWhenNicknameExists)
                        name = $"{receivedMessage.Nickname} ({receivedMessage.AuthorName})";
                    else
                        name = receivedMessage.Nickname;
                }
                else if (receivedMessage.AuthorName != null)
                {
                    name = receivedMessage.AuthorName;
                }

                receivedMessage.ChannelName = Configuration.ChannelMappings.TryGetValue(receivedMessage.Channel, out var channelName)
                    ? channelName
                    : receivedMessage.Channel;

                seString.Append(new UIForegroundPayload(35));
                seString.Append($"[{receivedMessage.ChannelName}] ");
                seString.Append(UIForegroundPayload.UIForegroundOff);
                seString.Append(new UIForegroundPayload(nameColor));
                seString.Append(name);
                seString.Append(UIForegroundPayload.UIForegroundOff);
                seString.Append(new UIForegroundPayload(1));
                seString.Append($": {receivedMessage.Content}");
                var stickerData = "";
                if (receivedMessage.StickerId != null)
                    stickerData = $" [{receivedMessage.StickerName}](https://media.discordapp.net/stickers/{receivedMessage.StickerId}.webp?size=160&quality=lossless)";
                if(stickerData == "" && receivedMessage.Content == null)
                    return;
                if (stickerData != "")
                {
                    seString.Append(new UIForegroundPayload(25));
                    seString.Append(stickerData);
                }
                seString.Append(UIForegroundPayload.UIForegroundOff);

                ChatGui.Print(seString);
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to process message: {ex.Message}");
            }
        };
    });
}


    private void StopWebSocketServer()
    {
        if (webSocketServer != null)
        {
            PluginLog.Information("Stopping WebSocket server...");

            foreach (var socket in connectedClients)
            {
                if (socket.IsAvailable)
                {
                    socket.Close();
                }
            }

            webSocketServer.Dispose();
            cancellationTokenSource.Cancel();
            PluginLog.Information("WebSocket server stopped.");
        }
    }
    
    private int GetNameColor(string name)
    {
        var index = Math.Abs(name.GetHashCode()) % nameColor.Length;
        return nameColor[index];
    }
    public void ToggleConfigUI() => ConfigWindow.Toggle();
    private void DrawUI() => WindowSystem.Draw();
}
