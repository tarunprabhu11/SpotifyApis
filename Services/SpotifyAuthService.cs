using Newtonsoft.Json;
using Spotify.Models;
using System.Net.Http.Headers;
using System.Text;

namespace Spotify.Services
{
    public class SpotifyAuthService
    {
        private readonly IConfiguration _config;
        private SpotifyTokenResponse _tokens;

        public SpotifyAuthService(IConfiguration config)
        {
            _config = config;
        }

        public string GetAuthorizationUrl()
        {
            var clientId = _config["Spotify:ClientId"];
            var redirectUri = _config["Spotify:RedirectUri"];
            var scope = "user-read-playback-state user-modify-playback-state user-read-currently-playing user-follow-read user-top-read streaming";
            return $"https://accounts.spotify.com/authorize?client_id={clientId}&response_type=code&redirect_uri={redirectUri}&scope={Uri.EscapeDataString(scope)}";
        }

        public async Task ExchangeCodeForTokenAsync(string code)
        {
            var client = new HttpClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config["Spotify:ClientId"]}:{_config["Spotify:ClientSecret"]}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var values = new Dictionary<string, string>
            {
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", _config["Spotify:RedirectUri"] }
            };

            var response = await client.PostAsync("https://accounts.spotify.com/api/token", new FormUrlEncodedContent(values));
            var content = await response.Content.ReadAsStringAsync();
            _tokens = JsonConvert.DeserializeObject<SpotifyTokenResponse>(content);
        }

        private async Task RefreshTokenAsync()
        {
            if (string.IsNullOrEmpty(_tokens?.Refresh_token)) return;

            var client = new HttpClient();
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config["Spotify:ClientId"]}:{_config["Spotify:ClientSecret"]}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var values = new Dictionary<string, string>
            {
                { "grant_type", "refresh_token" },
                { "refresh_token", _tokens.Refresh_token }
            };

            var response = await client.PostAsync("https://accounts.spotify.com/api/token", new FormUrlEncodedContent(values));
            var content = await response.Content.ReadAsStringAsync();
            _tokens = JsonConvert.DeserializeObject<SpotifyTokenResponse>(content);
        }

        public async Task<string> GetAccessTokenAsync()
        {
            if (_tokens == null)
                throw new Exception("Spotify not authenticated.");

            await RefreshTokenAsync();
            return _tokens.Access_token;
        }
    }
}
