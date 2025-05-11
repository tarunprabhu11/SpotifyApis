# Spotify API Integration

This project integrates the Spotify Web API into a portfolio website, providing endpoints to display a user's top 10 tracks, currently playing song, followed artists, and controls to play or pause tracks. The API is built using ASP.NET Core and deployed on Azure.

## Features

- **Top Tracks**: Retrieve and display the user's top 10 tracks based on long-term listening history.
- **Now Playing**: Show details of the currently playing track, including artist, album, and playback progress.
- **Followed Artists**: List artists followed by the user, including their genres and images.
- **Playback Controls**: Options to play any of the top 10 tracks or pause the current track.
- **Authentication**: Secure OAuth 2.0 flow for Spotify authentication.

## Endpoints

- `GET /spotify/login`: Redirects to Spotify's authorization page.
- `GET /spotify/callback`: Handles the OAuth callback and exchanges the authorization code for an access token.
- `GET /spotify/top-tracks`: Returns a JSON list of the user's top 10 tracks.
- `GET /spotify/now-playing`: Returns a JSON object with details of the currently playing track.
- `GET /spotify/followed-artists`: Returns a JSON list of artists followed by the user.
- `PUT /spotify/play/{trackId}`: Starts playback of a specified track by its Spotify ID.
- `PUT /spotify/pause`: Pauses the current playback.

## Prerequisites

- .NET 8.0 SDK
- Spotify Developer Account and Application (for Client ID and Client Secret)
- Azure account for deployment

## Setup

1. **Clone the Repository**:

   ```bash
   git clone https://github.com/your-username/spotify-api-integration.git
   cd spotify-api-integration
   ```

2. **Configure Spotify Credentials**:

   - Update `appsettings.json` with your Spotify API credentials:

     ```json
     "Spotify": {
       "ClientId": "your-client-id",
       "ClientSecret": "your-client-secret",
       "RedirectUri": "https://your-azure-app-url/spotify/callback"
     }
     ```

3. **Restore Dependencies**:

   ```bash
   dotnet restore
   ```

4. **Run Locally** (for testing):

   ```bash
   dotnet run
   ```

## Deployment

The project is deployed on Azure at:\
**https://cactrobe-h5fteba4auc8dpfy.eastasia-01.azurewebsites.net/spotify**

## Usage

1. Navigate to `/spotify/login` to authenticate with Spotify.
2. After authentication, access the endpoints:
   - `/spotify/top-tracks`: View your top 10 tracks.
   - `/spotify/now-playing`: Check the currently playing song.
   - `/spotify/followed-artists`: List followed artists.
   - `/spotify/play/{trackId}`: Play a track (use track IDs from top-tracks).
   - `/spotify/pause`: Pause playback.

Example response for `/spotify/top-tracks`:

```json
[
  {
    "Name": "Song Title",
    "Id": "spotify-track-id",
    "Artists": ["Artist 1", "Artist 2"],
    "Album": "Album Name",
    "PreviewUrl": "https://preview-url"
  },
  ...
]
```

## Project Structure

- **Controllers/SpotifyController.cs**: Defines API endpoints for Spotify interactions.
- **Services/SpotifyAuthService.cs**: Handles Spotify OAuth authentication and token management.
- **Models/SpotifyTokenResponse.cs**: Model for Spotify token response.
- **appsettings.json**: Configuration file for Spotify credentials and other settings.

## Dependencies

1. ASP.NET Core 8.0
2. Newtonsoft.Json
3. Microsoft.Extensions.Configuration

## License

This project is licensed under the MIT License.
