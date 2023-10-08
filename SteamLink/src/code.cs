using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class CPHInline
{
    private const string AUTH_ENDPOINT = "https://id.twitch.tv/oauth2/token";
    private const string IGDB_SEARCH_ENDPOINT = "https://api.igdb.com/v4/games";
    private const int STEAM_CATEGORY_ID = 1;
    public bool Execute()
    {
        string clientId = args["twitchClientId"]?.ToString();
        string clientSecret = args["twitchClientSecret"]?.ToString();
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
        {
            CPH.SendMessage("Please configure your Twitch Client ID and Client Secret for authentication before using this action");
            return true;
        }

        string gameTitle = args["game"]?.ToString();
        string gameId = args["gameId"]?.ToString();
        if (string.IsNullOrEmpty(gameTitle))
        {
            CPH.SendMessage("Game category not set?");
            return true;
        }

        if (gameTitle.ToLower() == "just chatting")
        {
            CPH.SendMessage("You're in the Just Chatting category!");
            return true;
        }

        Task<KeyValuePair<string, string>?> fetchTask = FetchSteamUrlFromIGDB(clientId, clientSecret, gameTitle, gameId);
        fetchTask.Wait();
        var result = fetchTask.Result;
        if (result != null)
        {
            if (result.Value.Key == gameTitle)
            {
                CPH.SendMessage($"Steam URL for {result.Value.Key}: {result.Value.Value}");
            }
            else
            {
                CPH.SendMessage($"Couldn't find {gameTitle}, but found {result.Value.Key} with this steam link: {result.Value.Value}");
            }
        }
        else
        {
            CPH.SendMessage($"Steam URL not found for {gameTitle}.");
        }

        return true;
    }

    static async Task<KeyValuePair<string, string>?> FetchSteamUrlFromIGDB(string clientId, string clientSecret, string gameTitle, string gameId)
    {
        using (HttpClient client = new HttpClient())
        {
            string authEndpoint = $"{AUTH_ENDPOINT}?client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials";
            HttpResponseMessage authResponse = await client.PostAsync(authEndpoint, null).ConfigureAwait(false);
            string authResult = await authResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            string accessToken = JObject.Parse(authResult)["access_token"].ToString();
            client.DefaultRequestHeaders.Add("Client-ID", clientId);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            string queryFields = "fields external_games.url, external_games.uid, name; where external_games.category = 1;";
            // Try fetching by gameId
            if (!string.IsNullOrEmpty(gameId))
            {
                var idPayload = new StringContent($"{queryFields} where external_games.uid = \"{gameId}\";", System.Text.Encoding.UTF8, "text/plain");
                var idResponse = await FetchUrlFromIGDBHelper(client, idPayload).ConfigureAwait(false);
                if (idResponse != null)
                {
                    return idResponse;
                }
            }

            // Try fetching by gameTitle
            var titlePayload = new StringContent($"{queryFields} search \"{gameTitle}\";", System.Text.Encoding.UTF8, "text/plain");
            return await FetchUrlFromIGDBHelper(client, titlePayload).ConfigureAwait(false);
        }
    }

    static async Task<KeyValuePair<string, string>?> FetchUrlFromIGDBHelper(HttpClient client, StringContent payload)
    {
        HttpResponseMessage response = await client.PostAsync(IGDB_SEARCH_ENDPOINT, payload).ConfigureAwait(false);
        if (response == null || response.Content == null)
        {
            return null;
        }

        string result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(result))
        {
            return null;
        }

        JArray games = JArray.Parse(result);
        if (games == null || games.Count == 0)
        {
            return null;
        }

        foreach (var game in games)
        {
            string foundGameTitle = game["name"]?.ToString();
            var externalGame = game["external_games"]?.First;
            if (externalGame == null)
            {
                return null;
            }

            string steamUrl = externalGame["url"]?.ToString();
            if (!string.IsNullOrEmpty(steamUrl))
            {
                return new KeyValuePair<string, string>(foundGameTitle, steamUrl);
            }
            else
            {
                // Construct Steam URL from UID
                string uid = externalGame["uid"]?.ToString();
                if (!string.IsNullOrEmpty(uid))
                {
                    steamUrl = $"https://store.steampowered.com/app/{uid}";
                    return new KeyValuePair<string, string>(foundGameTitle, steamUrl);
                }
            }
        }

        return null;
    }
}