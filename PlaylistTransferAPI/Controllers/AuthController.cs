using Microsoft.AspNetCore.Mvc;
using PlaylistTransferAPI.Models.Responses;
using System.Web;

namespace PlaylistTransferAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Spotify OAuth authorization başlatır
    /// </summary>
    [HttpGet("spotify/authorize")]
    public IActionResult SpotifyAuthorize()
    {
        try
        {
            var clientId = _configuration["SPOTIFY_CLIENT_ID"];
            var redirectUri = "https://tunesync.onrender.com/auth/spotify/callback";
            var state = Guid.NewGuid().ToString("N")[..16]; // CSRF protection

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("Spotify configuration is missing"));
            }

            var scopes = "playlist-read-private playlist-read-collaborative user-read-private user-read-email";
            
            var authUrl = "https://accounts.spotify.com/authorize?" +
                $"client_id={Uri.EscapeDataString(clientId)}&" +
                $"response_type=code&" +
                $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                $"scope={Uri.EscapeDataString(scopes)}&" +
                $"state={state}&" +
                "show_dialog=true";

            // Store state in session/cache for validation
            HttpContext.Session.SetString($"spotify_state_{state}", DateTime.UtcNow.ToString());

            return Ok(ApiResponse<object>.SuccessResponse(new { authUrl, state }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Spotify auth URL");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Failed to create authorization URL"));
        }
    }

    /// <summary>
    /// YouTube OAuth authorization başlatır
    /// </summary>
    [HttpGet("youtube/authorize")]
    public IActionResult YouTubeAuthorize()
    {
        try
        {
            var clientId = _configuration["YOUTUBE_CLIENT_ID"];
            var redirectUri = "https://tunesync.onrender.com/auth/youtube/callback";
            var state = Guid.NewGuid().ToString("N")[..16]; // CSRF protection

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("YouTube configuration is missing"));
            }

            var scopes = "https://www.googleapis.com/auth/youtube";
            
            var authUrl = "https://accounts.google.com/o/oauth2/v2/auth?" +
                $"client_id={Uri.EscapeDataString(clientId)}&" +
                $"response_type=code&" +
                $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
                $"scope={Uri.EscapeDataString(scopes)}&" +
                $"state={state}&" +
                $"access_type=offline&" +
                "prompt=consent";

            // Store state in session/cache for validation
            HttpContext.Session.SetString($"youtube_state_{state}", DateTime.UtcNow.ToString());

            return Ok(ApiResponse<object>.SuccessResponse(new { authUrl, state }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating YouTube auth URL");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Failed to create authorization URL"));
        }
    }

    /// <summary>
    /// Spotify OAuth callback handler
    /// </summary>
    [HttpGet("spotify/callback")]
    public async Task<IActionResult> SpotifyCallback([FromQuery] string code, [FromQuery] string state, [FromQuery] string error)
    {
        try
        {
            if (!string.IsNullOrEmpty(error))
            {
                return Redirect($"/?error=spotify_auth_denied&message={Uri.EscapeDataString(error)}");
            }

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                return Redirect("/?error=spotify_invalid_callback");
            }

            // Validate state (CSRF protection)
            var storedState = HttpContext.Session.GetString($"spotify_state_{state}");
            if (string.IsNullOrEmpty(storedState))
            {
                return Redirect("/?error=spotify_invalid_state");
            }

            // Exchange code for access token
            var tokenData = await ExchangeSpotifyCodeForToken(code);
            if (tokenData == null)
            {
                return Redirect("/?error=spotify_token_exchange_failed");
            }

            // Clean up state
            HttpContext.Session.Remove($"spotify_state_{state}");

            // Redirect back to frontend with tokens
            var redirectUrl = $"/?spotify_access_token={Uri.EscapeDataString(tokenData.AccessToken)}&" +
                            $"spotify_refresh_token={Uri.EscapeDataString(tokenData.RefreshToken ?? "")}&" +
                            $"spotify_expires_in={tokenData.ExpiresIn}";

            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Spotify callback");
            return Redirect("/?error=spotify_callback_error");
        }
    }

    /// <summary>
    /// YouTube OAuth callback handler
    /// </summary>
    [HttpGet("youtube/callback")]
    public async Task<IActionResult> YouTubeCallback([FromQuery] string code, [FromQuery] string state, [FromQuery] string error)
    {
        try
        {
            if (!string.IsNullOrEmpty(error))
            {
                return Redirect($"/?error=youtube_auth_denied&message={Uri.EscapeDataString(error)}");
            }

            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
            {
                return Redirect("/?error=youtube_invalid_callback");
            }

            // Validate state (CSRF protection)
            var storedState = HttpContext.Session.GetString($"youtube_state_{state}");
            if (string.IsNullOrEmpty(storedState))
            {
                return Redirect("/?error=youtube_invalid_state");
            }

            // Exchange code for access token
            var tokenData = await ExchangeYouTubeCodeForToken(code);
            if (tokenData == null)
            {
                return Redirect("/?error=youtube_token_exchange_failed");
            }

            // Clean up state
            HttpContext.Session.Remove($"youtube_state_{state}");

            // Redirect back to frontend with tokens
            var redirectUrl = $"/?youtube_access_token={Uri.EscapeDataString(tokenData.AccessToken)}&" +
                            $"youtube_refresh_token={Uri.EscapeDataString(tokenData.RefreshToken ?? "")}&" +
                            $"youtube_expires_in={tokenData.ExpiresIn}";

            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in YouTube callback");
            return Redirect("/?error=youtube_callback_error");
        }
    }

    private async Task<TokenResponse?> ExchangeSpotifyCodeForToken(string code)
    {
        try
        {
            var clientId = _configuration["SPOTIFY_CLIENT_ID"];
            var clientSecret = _configuration["SPOTIFY_CLIENT_SECRET"];
            var redirectUri = "https://tunesync.onrender.com/auth/spotify/callback";

            using var httpClient = new HttpClient();
            
            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri!)
            });

            var authValue = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authValue);

            var response = await httpClient.PostAsync("https://accounts.spotify.com/api/token", tokenRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Spotify token exchange failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenData = System.Text.Json.JsonSerializer.Deserialize<SpotifyTokenResponse>(responseContent);

            return new TokenResponse
            {
                AccessToken = tokenData?.access_token ?? "",
                RefreshToken = tokenData?.refresh_token,
                ExpiresIn = tokenData?.expires_in ?? 3600
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging Spotify code for token");
            return null;
        }
    }

    private async Task<TokenResponse?> ExchangeYouTubeCodeForToken(string code)
    {
        try
        {
            var clientId = _configuration["YOUTUBE_CLIENT_ID"];
            var clientSecret = _configuration["YOUTUBE_CLIENT_SECRET"];
            var redirectUri = "https://tunesync.onrender.com/auth/youtube/callback";

            using var httpClient = new HttpClient();
            
            var tokenRequest = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", redirectUri!),
                new KeyValuePair<string, string>("client_id", clientId!),
                new KeyValuePair<string, string>("client_secret", clientSecret!)
            });

            var response = await httpClient.PostAsync("https://oauth2.googleapis.com/token", tokenRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("YouTube token exchange failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenData = System.Text.Json.JsonSerializer.Deserialize<YouTubeTokenResponse>(responseContent);

            return new TokenResponse
            {
                AccessToken = tokenData?.access_token ?? "",
                RefreshToken = tokenData?.refresh_token,
                ExpiresIn = tokenData?.expires_in ?? 3600
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging YouTube code for token");
            return null;
        }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }

    public class SpotifyTokenResponse
    {
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
        public int expires_in { get; set; }
        public string? token_type { get; set; }
        public string? scope { get; set; }
    }

    public class YouTubeTokenResponse
    {
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
        public int expires_in { get; set; }
        public string? token_type { get; set; }
        public string? scope { get; set; }
    }
}