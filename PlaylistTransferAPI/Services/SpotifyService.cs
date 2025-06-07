using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using PlaylistTransferAPI.Models.DTOs;
using PlaylistTransferAPI.Models.Entities;
using PlaylistTransferAPI.Services.Interfaces;
using System.Text;

namespace PlaylistTransferAPI.Services;

public class SpotifyService : ISpotifyService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SpotifyService> _logger;
    private const string BaseUrl = "https://api.spotify.com/v1";

    public SpotifyService(HttpClient httpClient, IMemoryCache cache, ILogger<SpotifyService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<SpotifyPlaylistDto>> GetUserPlaylistsAsync(string accessToken, int limit = 50, int offset = 0)
    {
        try
        {
            var cacheKey = $"spotify_playlists_{accessToken[^8..]}_{limit}_{offset}";
            if (_cache.TryGetValue(cacheKey, out List<SpotifyPlaylistDto>? cachedPlaylists))
            {
                return cachedPlaylists!;
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var url = $"{BaseUrl}/me/playlists?limit={limit}&offset={offset}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Spotify API error: {StatusCode} - {Content}", 
                    response.StatusCode, await response.Content.ReadAsStringAsync());
                throw new HttpRequestException($"Spotify API error: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var playlistResponse = JsonConvert.DeserializeObject<SpotifyPlaylistResponse>(content);
            
            if (playlistResponse?.Items == null)
            {
                return new List<SpotifyPlaylistDto>();
            }

            // Cache for 5 minutes
            _cache.Set(cacheKey, playlistResponse.Items, TimeSpan.FromMinutes(5));
            
            return playlistResponse.Items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user playlists from Spotify");
            throw;
        }
    }

    public async Task<SpotifyPlaylistDetailsDto?> GetPlaylistDetailsAsync(string playlistId, string accessToken)
    {
        try
        {
            var cacheKey = $"spotify_playlist_{playlistId}";
            if (_cache.TryGetValue(cacheKey, out SpotifyPlaylistDetailsDto? cachedPlaylist))
            {
                return cachedPlaylist;
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var url = $"{BaseUrl}/playlists/{playlistId}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Spotify API error getting playlist {PlaylistId}: {StatusCode}", 
                    playlistId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var playlist = JsonConvert.DeserializeObject<SpotifyPlaylistDetailsDto>(content);

            // Cache for 10 minutes
            if (playlist != null)
            {
                _cache.Set(cacheKey, playlist, TimeSpan.FromMinutes(10));
            }

            return playlist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting playlist details for {PlaylistId}", playlistId);
            throw;
        }
    }

    public async Task<List<SpotifyTrackDto>> GetPlaylistTracksAsync(string playlistId, string accessToken)
    {
        try
        {
            var allTracks = new List<SpotifyTrackDto>();
            var offset = 0;
            const int limit = 100;
            string? nextUrl = null;

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            do
            {
                var url = nextUrl ?? $"{BaseUrl}/playlists/{playlistId}/tracks?limit={limit}&offset={offset}&fields=items(track(id,name,artists(id,name),album(id,name,release_date),duration_ms,external_ids,popularity)),total,limit,offset,next";
                
                var response = await _httpClient.GetAsync(url);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Spotify API error getting tracks for playlist {PlaylistId}: {StatusCode}", 
                        playlistId, response.StatusCode);
                    break;
                }

                var content = await response.Content.ReadAsStringAsync();
                var tracksResponse = JsonConvert.DeserializeObject<SpotifyTracksResponse>(content);

                if (tracksResponse?.Items == null) break;

                foreach (var item in tracksResponse.Items)
                {
                    if (item.Track?.Id != null) // Null track kontrolü (silinmiş şarkılar için)
                    {
                        allTracks.Add(item.Track);
                    }
                }

                nextUrl = tracksResponse.Next;
                offset += limit;

                // Rate limiting için kısa bekleme
                await Task.Delay(100);

            } while (!string.IsNullOrEmpty(nextUrl));

            _logger.LogInformation("Retrieved {TrackCount} tracks from playlist {PlaylistId}", 
                allTracks.Count, playlistId);

            return allTracks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting playlist tracks for {PlaylistId}", playlistId);
            throw;
        }
    }

    public Track ConvertToTrack(SpotifyTrackDto spotifyTrack)
    {
        return new Track
        {
            Id = Guid.NewGuid().ToString(),
            Name = spotifyTrack.Name,
            Artists = spotifyTrack.Artists.Select(a => new Artist
            {
                Id = a.Id,
                Name = a.Name,
                SpotifyId = a.Id
            }).ToList(),
            Album = spotifyTrack.Album.Name,
            DurationMs = spotifyTrack.DurationMs,
            SpotifyId = spotifyTrack.Id,
            IsMatched = false,
            MatchConfidence = 0.0
        };
    }

    public Playlist ConvertToPlaylist(SpotifyPlaylistDetailsDto spotifyPlaylist, List<Track> tracks)
    {
        return new Playlist
        {
            Id = Guid.NewGuid().ToString(),
            Name = spotifyPlaylist.Name,
            Description = spotifyPlaylist.Description,
            Owner = spotifyPlaylist.Owner.DisplayName,
            Tracks = tracks,
            TotalTracks = tracks.Count,
            ImageUrl = spotifyPlaylist.Images.FirstOrDefault()?.Url ?? string.Empty,
            IsPublic = spotifyPlaylist.Public ?? false,
            SpotifyId = spotifyPlaylist.Id
        };
    }

    public async Task<bool> ValidateAccessTokenAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync($"{BaseUrl}/me");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Spotify access token");
            return false;
        }
    }

    public async Task<SpotifyUserProfileDto?> GetUserProfileAsync(string accessToken)
    {
        try
        {
            var cacheKey = $"spotify_profile_{accessToken[^8..]}";
            if (_cache.TryGetValue(cacheKey, out SpotifyUserProfileDto? cachedProfile))
            {
                return cachedProfile;
            }

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync($"{BaseUrl}/me");
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var profile = JsonConvert.DeserializeObject<SpotifyUserProfileDto>(content);

            // Cache for 15 minutes
            if (profile != null)
            {
                _cache.Set(cacheKey, profile, TimeSpan.FromMinutes(15));
            }

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Spotify user profile");
            return null;
        }
    }
}