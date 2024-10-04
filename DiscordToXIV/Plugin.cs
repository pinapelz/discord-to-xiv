using System;
using Fleck;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Threading;
using System.Text.Json;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using DiscordToXIV.Windows;
using DiscordTOXIV.Windows;


namespace DiscordToXIV;


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
    private bool IsWebSocketServerRunning = false;

    private WebSocketServer webSocketServer;
    private DiscordMessenger discordMessenger;
    private string selectedChannelID = "";
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly List<IWebSocketConnection> connectedClients;
    public Configuration Configuration { get; init; }
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    public readonly WindowSystem WindowSystem = new("DiscordToXIV");
    

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        cancellationTokenSource = new CancellationTokenSource();
        connectedClients = new List<IWebSocketConnection>();
        discordMessenger = new DiscordMessenger(this, Configuration.DiscordAuthToken);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Start WebSocket server. Usage: /pdiscordtoxiv [port]"
        });
        
        CommandManager.AddHandler("/pdtxset", new CommandInfo(SetChannelCommand)
        {
            HelpMessage = "Set a channel ID to send messages to. Usage: /pdtxset <channel_id>"
        });
        
        CommandManager.AddHandler("/pdtxsend", new CommandInfo(SendMessageCommand)
        {
            HelpMessage = "Send a message to the selected channel. Usage: /pdtxs <message>"
        });
        
        
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;
        if(!Configuration.HideWelcomeMessage)
            ChatGui.Print("[DiscordToXIV] Websocket Server Ready! Use /pdiscordtoxiv <port> to start the server (Need help? /pdiscordtoxiv help)");
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
                ChatGui.Print("[DiscordToXIV] WebSocket server stop requested");
            }
            else if (args == "start")
            {
                ChatGui.Print("[DiscordToXIV] Starting WebSocket server with default port " + DefaultPort);
                StartWebSocketServer(DefaultPort);
            }
            else if (args == "help")
                ToggleMainUI();
            else if(args == "config")
                ToggleConfigUI();
            else if (!int.TryParse(args, out port) || port <= 0 || port > 65535)
            {
                ChatGui.PrintError($"[DiscordToXIV] Invalid port number: {args}. Using default port {DefaultPort}.");
                StartWebSocketServer(DefaultPort);
            }
            else
            {
                ToggleMainUI(); 
            }
            return;
        }
        
        // No args provided
        if (!IsWebSocketServerRunning)
        {
            ChatGui.Print("[DiscordToXIV] Starting WebSocket server with default port " + DefaultPort);
            StartWebSocketServer(DefaultPort);
        }
        else
        {
            ChatGui.Print("[DiscordToXIV] Stopping WebSocket server...");
            StopWebSocketServer();
        }

    }
    
    private void SendMessageCommand(string command, string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            ChatGui.PrintError("[DiscordToXIV] No message provided.");
            return;
        }

        if (string.IsNullOrEmpty(selectedChannelID))
        {
            ChatGui.PrintError("[DiscordToXIV] No channel selected.");
            return;
        }

        discordMessenger.SendMessageAsync(args, selectedChannelID);
    }
    
    private void SetChannelCommand(string command, string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            ChatGui.PrintError("[DiscordToXIV] No channel ID provided.");
            return;
        }

        if (Configuration.ChannelMappings.ContainsKey(args))
        {
            selectedChannelID = args;
            ChatGui.Print("[DiscordToXIV] Channel set to " + Configuration.ChannelMappings[args]);
        }
        else
        {
            ChatGui.PrintError("[DiscordToXIV] Channel ID not found in configuration.");
        }
    }

    private void StartWebSocketServer(int port)
    {
        StopWebSocketServer();
        IsWebSocketServerRunning = true;
        webSocketServer = new WebSocketServer($"ws://0.0.0.0:{port}");
        webSocketServer.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                PluginLog.Information("[DiscordToXIV] WebSocket connection opened.");
                ChatGui.Print("[DiscordToXIV] Connected to BetterDiscord Relayer!");
                connectedClients.Add(socket);
            };

            socket.OnClose = () =>
            {
                PluginLog.Information("[DiscordToXIV] WebSocket connection closed.");
                ChatGui.Print("[DiscordToXIV] Disconnected from BetterDiscord Relayer!");
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
                    

                    var seString = new SeString(new List<Payload>());
                    var name = receivedMessage.Author;
                    ushort nameColor = (ushort)ChatUtils.GetNameColor(name, this.nameColor);
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
                    
                    if(Configuration.ChannelMappings.TryGetValue(receivedMessage.Channel, out var channelName))
                        receivedMessage.ChannelName = channelName;
                    else
                    {
                        if (Configuration.ShowOnlyKnownChannels) return;
                        receivedMessage.ChannelName = receivedMessage.Channel;
                    }

                    seString.Append(new UIForegroundPayload(56));
                    seString.Append($"[{receivedMessage.ChannelName}] ");
                    seString.Append(UIForegroundPayload.UIForegroundOff);
                    seString.Append(new UIForegroundPayload(nameColor));
                    seString.Append(name);
                    seString.Append(UIForegroundPayload.UIForegroundOff);
                    seString.Append(new UIForegroundPayload(1));
                    var messageContent = receivedMessage.Content;
                    if (Configuration.AdjustEmoteText)
                        messageContent = ChatUtils.FixEmoteText(messageContent);
                    if (Configuration.AdjustMentions)
                        messageContent = ChatUtils.ReplaceMentionsWithNames(messageContent, receivedMessage.Mentions);
                    seString.Append($": {messageContent}");
                    var stickerData = "";
                    if (receivedMessage.StickerId != null)
                        if (!Configuration.HideStickerUrls)
                        {
                            stickerData = $" [{receivedMessage.StickerName}](https://media.discordapp.net/stickers/{receivedMessage.StickerId}.webp?size=160&quality=lossless)";
                        }
                        else
                        {
                            stickerData = $" [{receivedMessage.StickerName}]";
                        }
                    var attachments = receivedMessage.Attachments;
                    if (attachments != null)
                    {
                        foreach (var attachment in attachments)
                        {
                            seString.Append(new UIForegroundPayload(25));
                            if (!Configuration.HideAttachmentUrls)
                            {
                                seString.Append($"\nAttachment:\n[{attachment.Filename}]({attachment.Url})");
                            }
                            else
                            {
                                seString.Append($"\nAttachment:\n{attachment.Filename}");
                            }
                            seString.Append(UIForegroundPayload.UIForegroundOff);
                        }
                    }
                    seString.Append(new UIForegroundPayload(25));
                    seString.Append(stickerData);
                    if(stickerData == "" && receivedMessage.Content == null) return;
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
            IsWebSocketServerRunning = false;
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
    
    public bool GetIsWebSocketServerRunning() => IsWebSocketServerRunning;
    

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
    private void DrawUI() => WindowSystem.Draw();
}
