using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Text;
using Newtonsoft.Json;

public class CPHInline
{
    public bool Execute()
    {
        string discordWebhookURL = args["discordRaidWebhookURL"].ToString();

        string displayName = args["user"].ToString();
        string userName = args["userName"].ToString();
        string userGame = args["game"].ToString();
		    string streamerName = String.IsNullOrWhiteSpace(args["streamerName"].ToString()) ? args["broadcastUser"].ToString() : args["streamerName"].ToString();

		    string defaultMessageContents = $"{streamerName} just got raided by {displayName}, which was very kind of them! They were streaming {userGame}. Lets return the kindness by heading over to https://twitch.tv/{userName} and giving them a follow!";
		    string messageContents = String.IsNullOrWhiteSpace(args["message"].ToString()) ? defaultMessageContents : ReplaceTokens(args["message"].ToString(), streamerName, displayName, userGame, userName);

        var message = new DiscordMessage(messageContents);

        string discordMessage = JsonConvert.SerializeObject(message);
        HttpClient client = new HttpClient();
        MultipartFormDataContent formData = new MultipartFormDataContent();
        formData.Add(new StringContent(discordMessage), "payload_json");

        client.PostAsync(discordWebhookURL, formData).Wait();
        client.Dispose();

        return true;
    }
    
    public string ReplaceTokens(string message, string streamerName, string displayName, string userGame, string userName) {
        message = message.Replace("{streamerName}", streamerName);
        message = message.Replace("{displayName}", displayName);
        message = message.Replace("{game}", userGame);
        message = message.Replace("{userName}", userName);
        return message;
    }
    
    internal class DiscordMessage
    {
		    public DiscordMessage(string message) {
			      content = message;
		    } 

        public string content { get; private set; }
    }
}
