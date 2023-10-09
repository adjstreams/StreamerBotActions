using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

public class CPHInline
{
    private const string AUTH_ENDPOINT = "https://id.twitch.tv/oauth2/token";
    private const string IGDB_SEARCH_ENDPOINT = "https://api.igdb.com/v4/games";
    private const int STEAM_CATEGORY_ID = 1;
    private const int TWITCH_CATEGORY_ID = 14;
    public bool Execute()
    {
        try
        {
            var configValid = ValidateConfiguration(out var clientId, out var clientSecret);
            if (!configValid)
            {
                return true;
            }

            var gameValid = ValidateGame(out var gameTitle);
            if (!gameValid)
            {
                return true;
            }

            var fetchTask = FetchSteamUrlFromIGDB(clientId, clientSecret, gameTitle);
            Task.Run(() => fetchTask).Wait();
            HandleFetchResult(fetchTask.Result, gameTitle);
        }
        catch (Exception ex)
        {
            CPH.LogError($"There was an error: {ex}");
            CPH.SendMessage("Sorry, try again later, there's an issue getting the steam link");
        }

        return true;
    }

    private bool ValidateConfiguration(out string clientId, out string clientSecret)
    {
        clientId = args["twitchClientId"]?.ToString();
        clientSecret = args["twitchClientSecret"]?.ToString();
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            CPH.SendMessage("Please configure your Twitch Client ID and Client Secret.");
            return false;
        }

        return true;
    }

    private bool ValidateGame(out string gameTitle)
    {
        gameTitle = args["game"]?.ToString();
        if (string.IsNullOrEmpty(gameTitle))
        {
            CPH.SendMessage("Game category not set?");
            return false;
        }

        if (gameTitle.ToLower() == "just chatting")
        {
            CPH.SendMessage("You're in the Just Chatting category!");
            return false;
        }

        return true;
    }

    private void HandleFetchResult(KeyValuePair<string, string>? result, string gameTitle)
    {
        if (result != null)
        {
            if (result.Value.Key == gameTitle)
            {
                CPH.SendMessage($"Steam URL for {result.Value.Key}: {result.Value.Value}");
                return;
            }
        }

        CPH.SendMessage($"Steam URL not found for {gameTitle}.");
        return;
    }

    private static async Task<KeyValuePair<string, string>?> FetchSteamUrlFromIGDB(string clientId, string clientSecret, string gameTitle)
    {
        using (var client = new HttpClient())
        {
            var accessToken = await GetAccessToken(client, clientId, clientSecret);
            client.DefaultRequestHeaders.Add("Client-ID", clientId);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            var payload = new StringContent($"fields external_games.url, external_games.category, name; where name ~ *\"{gameTitle}\"* & external_games.category = {STEAM_CATEGORY_ID};", System.Text.Encoding.UTF8, "text/plain");
            return await FetchUrlFromIGDBHelper(client, payload);
        }
    }

    private static async Task<string> GetAccessToken(HttpClient client, string clientId, string clientSecret)
    {
        var authEndpoint = $"{AUTH_ENDPOINT}?client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials";
        var authResponse = await client.PostAsync(authEndpoint, null).ConfigureAwait(false);
        var authResult = await authResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
        return JObject.Parse(authResult)["access_token"].ToString();
    }

    private static async Task<KeyValuePair<string, string>?> FetchUrlFromIGDBHelper(HttpClient client, StringContent payload)
    {
        var response = await client.PostAsync(IGDB_SEARCH_ENDPOINT, payload).ConfigureAwait(false);
        var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (string.IsNullOrEmpty(result))
        {
            return null;
        }

        var games = JArray.Parse(result);
        var firstGame = (JObject)games.First;
        var foundGameTitle = firstGame["name"]?.ToString();
        var externalGames = (JArray)firstGame["external_games"];
        var filteredGame = externalGames.FirstOrDefault(eg => eg["category"]?.Value<int>() == STEAM_CATEGORY_ID);
        if (filteredGame == null)
        {
            return null;
        }

        var steamUrl = filteredGame["url"]?.ToString();
        return !string.IsNullOrEmpty(steamUrl) ? new KeyValuePair<string, string>(foundGameTitle, steamUrl) : null;
    }
}