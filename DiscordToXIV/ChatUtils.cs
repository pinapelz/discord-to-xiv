using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text.Json.Serialization;
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

    [JsonPropertyName("mentions")]
    public List<Mention>? Mentions { get; init; }
    
    [JsonPropertyName("attachments")]
    public List<Attachment>? Attachments { get; set; }
}

public class Mention
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }
    
    [JsonPropertyName("username")]
    public string? Username { get; init; }
    
    [JsonPropertyName("avatar")]
    public string? Avatar { get; init; }
    
    [JsonPropertyName("discriminator")]
    public string? Discriminator { get; init; }
    
    [JsonPropertyName("bot")]
    public bool Bot { get; init; }
}

public class Attachment
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("filename")]
    public string? Filename { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("proxy_url")]
    public string? ProxyUrl { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("content_type")]
    public string? ContentType { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }
}

public class ChatUtils
{
    public static int GetNameColor(string name, int[] nameColor)
    {
        var index = Math.Abs(name.GetHashCode()) % nameColor.Length;
        return nameColor[index];
    }

    public static string? FixEmoteText(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
        string pattern = @"<a?:([^:]+):\d+>";
        return Regex.Replace(input, pattern, m => $" {m.Groups[1].Value}");
    }
    
    public static string? ReplaceMentionsWithNames(string input, List<Mention>? mentions)
    {
        if (string.IsNullOrEmpty(input)) return input;
        if (mentions == null || mentions.Count == 0) return input;
        foreach (var mention in mentions)
        {
            input = input.Replace($"<@{mention.Id}>", "@"+mention.Username);
        }
        return input;
    }
}

