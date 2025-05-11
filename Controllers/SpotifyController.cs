using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spotify.Services;
using System.Net.Http.Headers;
using System.Text;

namespace SpotifyIntegration.Controllers
{
    [Route("spotify")]
    [ApiController]
    public class SpotifyController : ControllerBase
    {
        private readonly SpotifyAuthService _authService;

        public SpotifyController(SpotifyAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            var authUrl = _authService.GetAuthorizationUrl();
            return Redirect(authUrl);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code)
        {
            await _authService.ExchangeCodeForTokenAsync(code);
            return Ok("Spotify account authenticated. You can now access /spotify endpoints.");
        }

        [HttpGet("top-tracks")]
        public async Task<IActionResult> GetTopTracks()
        {
            var token = await _authService.GetAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("https://api.spotify.com/v1/me/top/tracks?limit=10&time_range=long_term");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<JObject>(json);

            var items = data["items"] as JArray;

            var topTracks = items.Select(track => new
            {
                Name = track["name"]?.ToString(),
                Id = track["id"]?.ToString(),
                Artists = track["artists"]?.Select(a => a["name"]?.ToString()).ToList(),
                Album = track["album"]?["name"]?.ToString(),
                PreviewUrl = track["preview_url"]?.ToString()
            });

            return Ok(topTracks);
        }

        [HttpGet("now-playing")]
        public async Task<IActionResult> NowPlaying()
        {
            var token = await _authService.GetAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("https://api.spotify.com/v1/me/player/currently-playing");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { message = "Failed to fetch currently playing track.", details = error });
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<JObject>(json);

            if (data["item"] == null)
            {
                return Ok(new { message = "No track is currently playing." });
            }

            var track = data["item"];
            var artists = track["artists"]?.Select(a => a["name"]?.ToString()).ToList();

            var result = new
            {
                Name = track["name"]?.ToString(),
                Artists = artists,
                Album = track["album"]?["name"]?.ToString(),
                AlbumImage = track["album"]?["images"]?[0]?["url"]?.ToString(),
                ProgressMs = data["progress_ms"],
                DurationMs = track["duration_ms"],
                SpotifyUrl = track["external_urls"]?["spotify"]?.ToString(),
                IsPlaying = data["is_playing"]
            };

            return Ok(result);
        }


        [HttpGet("followed-artists")]
        public async Task<IActionResult> FollowedArtists()
        {
            var token = await _authService.GetAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("https://api.spotify.com/v1/me/following?type=artist");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, error);
            }

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<JObject>(json);

            var artists = data["artists"]?["items"] as JArray;

            var result = artists?.Select(artist => new
            {
                Id = artist["id"]?.ToString(),
                Name = artist["name"]?.ToString(),
                Genres = artist["genres"]?.Select(g => g.ToString()).ToList(),
                Url = artist["external_urls"]?["spotify"]?.ToString(),
                Images = artist["images"]?.Select(img => new {
                    Url = img["url"]?.ToString(),
                    Width = img["width"]?.ToObject<int?>(),
                    Height = img["height"]?.ToObject<int?>()
                }).ToList()
            });

            return Ok(result);
        }


        [HttpPut("play/{trackId}")]
        public async Task<IActionResult> Play(string trackId)
        {
            var token = await _authService.GetAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { uris = new[] { $"spotify:track:{trackId}" } };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await client.PutAsync("https://api.spotify.com/v1/me/player/play", content);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { message = "Track started playing successfully." });
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { message = "Failed to start track.", details = error });
            }
        }

        [HttpPut("pause")]
        public async Task<IActionResult> Pause()
        {
            var token = await _authService.GetAccessTokenAsync();
            var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.PutAsync("https://api.spotify.com/v1/me/player/pause", null);

            if (response.IsSuccessStatusCode)
            {
                return Ok(new { message = "Playback paused successfully." });
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, new { message = "Failed to pause playback.", details = error });
            }
        }

    }
}
