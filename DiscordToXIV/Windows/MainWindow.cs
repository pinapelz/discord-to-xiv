using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using DiscordToXIV;
using ImGuiNET;

namespace DiscordTOXIV.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;

    public MainWindow(Plugin plugin)
        : base("DiscordToXIV###DiscordToXIVMainWindow", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Text("DiscordToXIV");
        ImGui.Text("Nothing much here for now... Use /pdiscordtoxiv <port> to start the server");
        ImGui.Text("Stop the server: /pdiscordtoxiv stop");
    }
}
