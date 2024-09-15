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
        Configuration = plugin.Configuration;
    }

    public void Dispose() { }
public override void Draw()
{
    // Existing code for checkboxes...

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

            // Declare variables at the top of the loop
            string channelId = key;
            string channelName = Configuration.ChannelMappings[key];

            // Input for Channel ID
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
                    // Update the key in the dictionary
                    Configuration.ChannelMappings.Remove(key);
                    Configuration.ChannelMappings[channelId] = channelName;
                    Configuration.Save();
                    break; // Exit the loop to avoid enumeration issues
                }
            }

            ImGui.TableNextColumn();

            // Input for Channel Name
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

            // Delete button
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
}

}

