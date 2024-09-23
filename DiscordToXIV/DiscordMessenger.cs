using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscordToXIV
{
    public class DiscordMessenger
    {
        private readonly string _authorization;
        private readonly string _baseUrl;
        private readonly Plugin Plugin;

        public DiscordMessenger(Plugin plugin, string authorization = "", string baseUrl = "https://discord.com/api/v9/channels/")
        {
            Plugin = plugin;
            _authorization = authorization;
            _baseUrl = baseUrl;
        }

        public async Task SendMessageAsync(string message, string channelId)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create($"{_baseUrl}{channelId}/messages");
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers["Authorization"] = _authorization;
                var payload = $"{{\"content\":\"{message}\"}}";
                var data = Encoding.UTF8.GetBytes(payload);
                using (var stream = await request.GetRequestStreamAsync())
                {
                    await stream.WriteAsync(data, 0, data.Length);
                }
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        Plugin.ChatGui.PrintError("Failed to send message!");
                    }
                    else
                    {
                        Plugin.ChatGui.Print(">> " + Plugin.Configuration.ChannelMappings[channelId] + ": " + message);
                    }
                }
            }
            catch (WebException ex)
            {
                Plugin.ChatGui.PrintError($"Error sending message: {ex.Message}");
            }
        }
    }
}
