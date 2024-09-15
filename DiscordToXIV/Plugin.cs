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
using Dalamud.Interface.Windowing;
using DiscordToXIV.Windows;

namespace DiscordToXIV;


public class Message
{
    [JsonPropertyName("author")]
    public string Author { get; set; }

    [JsonPropertyName("author_name")]
    public string AuthorName { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; }

    [JsonPropertyName("time")]
    public string Time { get; set; }

    [JsonPropertyName("channel")]
    public string Channel { get; set; }
}



public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;

    private const string CommandName = "/pdiscordtoxiv";
    private const int DefaultPort = 8765;

    private WebSocketServer _webSocketServer;
    private CancellationTokenSource _cancellationTokenSource;
    private List<IWebSocketConnection> _connectedClients;
    public Configuration Configuration { get; init; }
    private ConfigWindow ConfigWindow { get; init; }
    public readonly WindowSystem WindowSystem = new("SamplePlugin");

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);
        _cancellationTokenSource = new CancellationTokenSource();
        _connectedClients = new List<IWebSocketConnection>();

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Start WebSocket server. Usage: /pdiscordtoxiv [port]"
        });
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

        _webSocketServer = new WebSocketServer($"ws://0.0.0.0:{port}");

        _webSocketServer.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                PluginLog.Information("WebSocket connection opened.");
                _connectedClients.Add(socket);
            };
            
            socket.OnClose = () =>
            {
                PluginLog.Information("WebSocket connection closed.");
                _connectedClients.Remove(socket);
            };
            
            socket.OnMessage = message =>
            {
                try
                {
                    Message receivedMessage = JsonSerializer.Deserialize<Message>(message);
                    //PluginLog.Information($"Message from {receivedMessage.Nickname}: {receivedMessage.Content}");
                    ChatGui.Print($"[{receivedMessage.AuthorName}] {receivedMessage.Nickname}: {receivedMessage.Content}");
                }
                catch (Exception ex)
                {
                    PluginLog.Error($"Failed to deserialize message: {ex.Message}");
                }
            };

        });
    }

    private void StopWebSocketServer()
    {
        if (_webSocketServer != null)
        {
            PluginLog.Information("Stopping WebSocket server...");

            foreach (var socket in _connectedClients)
            {
                if (socket.IsAvailable)
                {
                    socket.Close();
                }
            }

            _webSocketServer.Dispose();
            _cancellationTokenSource.Cancel();
            PluginLog.Information("WebSocket server stopped.");
        }
    }
    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
