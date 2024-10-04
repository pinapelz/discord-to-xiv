using System;
using System.Diagnostics;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using DiscordToXIV;
using FFXIVClientStructs.FFXIV.Component.Shell;
using ImGuiNET;

namespace DiscordTOXIV.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;

    public MainWindow(Plugin plugin)
        : base("DiscordToXIV###DiscordToXIVMainWindow")
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
        ImGui.SetWindowFontScale(1.5f);
        ImGui.Text("DiscordToXIV - Help");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.Text("Welcome. This mini-guide will help you get started with DiscordToXIV.");
        if (ImGui.Button("View this guide on GitHub (its more readable)"))
        {
            Process.Start(new ProcessStartInfo("https://github.com/pinapelz/discord-to-xiv/blob/master/SETUP.md")
            {
                UseShellExecute = true,
            });
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.SetWindowFontScale(1.2f);
        ImGui.Text("1. Set up BetterDiscord");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.Text("This plugin requires a Discord client with BetterDiscord so that Dalamud can communicate with it");
        ImGui.Text("Follow the instructions on their website, it'll patch into your existing Discord client");
        if (ImGui.Button("BetterDiscord Website"))
        {
            Process.Start(new ProcessStartInfo("https://betterdiscord.app/ ")
            {
                UseShellExecute = true,
            });
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.SetWindowFontScale(1.2f);
        ImGui.Text("2. Download and install the BetterDiscord plugin");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.Text("Install the BDFireToWebsocket plugin");
        ImGui.Text("This is required so Dalamud knows when you get a new message");
        ImGui.Text("Download the .js file from the link below and put it in your BetterDiscord plugins folder");
        ImGui.Text("You can access this in your BetterDiscord client by going to Settings -> Plugins -> Open Plugin Folder");
        if (ImGui.Button("BDFireToWebsocket Plugin"))
        {
            Process.Start(new ProcessStartInfo("https://github.com/pinapelz/BDFireToWebsocket/blob/7b2752d529cf3c6b5115c200aeb7b6f684ce807b/BDFireToWebsocket.plugin.js")
            {
                UseShellExecute = true,
            });
        }
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.SetWindowFontScale(1.2f);
        ImGui.Text("3. Configure BDFireToWebsocket Plugin");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.Text("Click on the settings icon for the BDFireToWebsocket plugin in BetterDiscord in the plugins menu");
        ImGui.Spacing();
        ImGui.Text("For Websocket address you can leave it as the default unless you know what you're doing");
        ImGui.Spacing();
        ImGui.Text("Channel IDs controls which messages are sent to Dalamud");
        ImGui.Text("I suggest leaving this empty and then filtering which channels you want from the Dalamud plugin instead");
        ImGui.Spacing();
        ImGui.Text("Make sure to add your own User ID so that messages you sent get filtered out! (otherwise you'll see your own messages in game)");
        ImGui.Text("You can get your User ID by right clicking on your name anywhere in Discord and clicking 'Copy ID'");
        
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.SetWindowFontScale(1.2f);
        ImGui.Text("4. Configure DiscordToXIV Plugin");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.Text("Head back into game and run '/pdiscordtoxiv config'");
        ImGui.Text("This will open the plugin settings window where you can configure the plugin to your liking");
        ImGui.Spacing();
        ImGui.Text("You'll need to set nicknames for each chat channel so their names don't show up as a bunch of numbers");
        ImGui.Text("You can get the channel ID by right clicking on any server channel or even DM channel and clicking 'Copy Channel ID'");
        ImGui.Text("Ex. #general-ffxiv -> 12345564323454");
        ImGui.Spacing();
        ImGui.Text("(Optional). You can also set up a Discord Auth Token if you want to send messages from the game to Discord");
        ImGui.Text("I won't get into how to get this token since its slightly more risky, but you can find guides online");
        ImGui.Text("If you do set it up. I'll have instructions on how to use it later down");
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.SetWindowFontScale(1.2f);
        ImGui.Text("5. Good to GO!");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.Text("Almost done now! run /pdiscordtoxiv");
        ImGui.Text("You should see a message in chat letting you know that the server is running");
        ImGui.Text("Head back into your BetterDiscord plugins and click the settings icon for BDToFireWebsocket again");
        ImGui.Text("Click 'Reconnect to Websocket Server'");
        ImGui.Spacing();
        ImGui.Text("You should now see a message in game letting you know that the connection is successful!");
        
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.SetWindowFontScale(1.2f);
        ImGui.Text("(Optional) 6. Sending Discord messages");
        ImGui.SetWindowFontScale(1.0f);
        ImGui.Text("If you set up the Discord Auth Token you can now send messages from the game to Discord");
        ImGui.Text("First you'll need to 'focus' on a chat channel by typing '/pdtxset <channel_id>'");
        ImGui.Text("You must use the numerical channel ID here not a nickname");
        ImGui.Spacing();
        ImGui.Text("Then you can send messages by typing '/pdtxsend <message>'");
        ImGui.Text("This will send the message to the focused channel");
        ImGui.Spacing();
        ImGui.Text("My suggestion for using this is to setup different macros to focus in on different channels");
        ImGui.Text("This will make it easier to send messages to different channels without having to type the channel ID each time");




    }
}
