using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
 
namespace DiscordToXIV.Windows;


public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigWindow(Plugin plugin) : base("DiscordToXIV Config###DiscordToXIVConfig")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500, 350),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
        Configuration = plugin.Configuration;
    }

    public void Dispose() { }
public override void Draw()
{
    ImGui.Spacing();

    if (ImGui.BeginTable("ChannelMappingsTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
    {
        ImGui.TableSetupColumn("Channel ID");
        ImGui.TableSetupColumn("Channel Name");
        ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, 60);
        ImGui.TableHeadersRow();

        string keyToRemove = null;
        int index = 0;

        var keys = new List<string>(Configuration.ChannelMappings.Keys);

        foreach (var key in keys)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            string channelId = key;
            string channelName = Configuration.ChannelMappings[key];
            
            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputText($"##ChannelID_{index}", ref channelId, 256))
            {
                channelId = channelId.Trim();

                if (string.IsNullOrEmpty(channelId))
                {
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), "Channel ID cannot be empty");
                }
                else if (channelId != key && Configuration.ChannelMappings.ContainsKey(channelId))
                {
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), "Channel ID already exists");
                }
                else
                {
                    Configuration.ChannelMappings.Remove(key);
                    Configuration.ChannelMappings[channelId] = channelName;
                    Configuration.Save();
                    break;
                }
            }

            ImGui.TableNextColumn();

            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputText($"##ChannelName_{index}", ref channelName, 256))
            {
                channelName = channelName.Trim();
                if (string.IsNullOrEmpty(channelName))
                {
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), "Channel Name cannot be empty");
                }
                else
                {
                    Configuration.ChannelMappings[channelId] = channelName;
                    Configuration.Save();
                }
            }

            ImGui.TableNextColumn();

            if (ImGui.Button($"Delete##{index}"))
            {
                keyToRemove = key;
            }

            index++;
        }

        if (keyToRemove != null)
        {
            Configuration.ChannelMappings.Remove(keyToRemove);
            Configuration.Save();
        }

        ImGui.EndTable();
    }

    if (ImGui.Button("Add New Mapping"))
    {
        int newIndex = 1;
        string newKey = "NewChannelID_" + newIndex;
        while (Configuration.ChannelMappings.ContainsKey(newKey))
        {
            newIndex++;
            newKey = "NewChannelID_" + newIndex;
        }

        Configuration.ChannelMappings.Add(newKey, "ChannelName");
        Configuration.Save();
    }
    
    ImGui.Separator();
    
    var hideUsernameWhenNicknameExists = Configuration.HideUsernameWhenNicknameExists;
    if (ImGui.Checkbox("Hide username when nickname exists", ref hideUsernameWhenNicknameExists))
    {
        Configuration.HideUsernameWhenNicknameExists = hideUsernameWhenNicknameExists;
        Configuration.Save();
    }
    
    var showOnlyKnownChannels =  Configuration.ShowOnlyKnownChannels;
    if(ImGui.Checkbox("Show only known channels", ref showOnlyKnownChannels))
    {
        Configuration.ShowOnlyKnownChannels = showOnlyKnownChannels;
        Configuration.Save();
    }
    
    var adjustEmoteText = Configuration.AdjustEmoteText;
    if(ImGui.Checkbox("Fix Emotes", ref adjustEmoteText))
    {
        Configuration.AdjustEmoteText = adjustEmoteText;
        Configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
        ImGui.BeginTooltip();
        ImGui.Text("Detects when an emote is used and adjusts the text to make it more readable");
        ImGui.EndTooltip();
    }
    
    ImGui.SameLine();

    var adjustMentionsText = Configuration.AdjustMentions;
    if (ImGui.Checkbox("Fix Mentions", ref adjustEmoteText))
    {
        Configuration.AdjustMentions = adjustMentionsText;
        Configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
        ImGui.BeginTooltip();
        ImGui.Text("Replaces mention User ID with name instead");
        ImGui.EndTooltip();
    }
    
    var hideWelcomeMessage = Configuration.HideWelcomeMessage;
    if (ImGui.Checkbox("Hide welcome message", ref hideWelcomeMessage))
    {
        Configuration.HideWelcomeMessage = hideWelcomeMessage;
        Configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
        ImGui.BeginTooltip();
        ImGui.Text("Hides plugin's welcome chat message");
        ImGui.EndTooltip();
    }

    var hideAttachmentUrls = Configuration.HideAttachmentUrls;
    if (ImGui.Checkbox("Hide attachment URLs", ref hideAttachmentUrls))
    {
        Configuration.HideAttachmentUrls = hideAttachmentUrls;
        Configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
        ImGui.BeginTooltip();
        ImGui.Text("Hides URLs of attachments in chat");
        ImGui.EndTooltip();
    }
    
    ImGui.SameLine();
    
    var hideStickerUrls = Configuration.HideStickerUrls;
    if (ImGui.Checkbox("Hide sticker URLs", ref hideStickerUrls))
    {
        Configuration.HideStickerUrls = hideStickerUrls;
        Configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
        ImGui.BeginTooltip();
        ImGui.Text("Hides URLs of stickers in chat");
        ImGui.EndTooltip();
    }


}

}

