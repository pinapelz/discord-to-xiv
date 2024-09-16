using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace DiscordToXIV;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public Dictionary<string, string> ChannelMappings { get; set; } = new Dictionary<string, string>();
    public bool HideUsernameWhenNicknameExists { get; set; } = false;
    public bool ShowOnlyKnownChannels { get; set; } = false;
    public bool AdjustEmoteText { get; set; } = true;
    public bool AdjustMentions { get; set; } = true;
    public bool HideWelcomeMessage { get; set; } = false;
    public bool HideAttachmentUrls { get; set; } = false;
    public bool HideStickerUrls { get; set; } = false;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
