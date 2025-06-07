using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using PlaylistTransferAPI.Models.DTOs;
using PlaylistTransferAPI.Models.Entities;
using PlaylistTransferAPI.Services.Interfaces;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

namespace PlaylistTransferAPI.Services;

public class YouTubeService : IYouTubeService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<YouTubeService> _logger;
    private const string BaseUrl = "https://www.googleapis.com/youtube/v3";

    public YouTubeService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration, ILogger<YouTubeService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<YouTubeVideoDto>> SearchVideosAsync(string query, int maxResults = 5)
    {
        try
        {
            var apiKey = _configuration["YouTube:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("YouTube API key not configured");
            }

            var cacheKey = $"youtube_search_{query.GetHashCode()}_{maxResults}";
            if (_cache.TryGetValue(cacheKey, out List<YouTubeVideoDto>? cachedResults))
            {
                return cachedResults!;
            }

            var encodedQuery = Uri.EscapeDataString(query);
            var url = $"{BaseUrl}/search?part=snippet&type=video&videoCategoryId=10&maxResults={maxResults}&q={encodedQuery}&key={apiKey}";

            var response = await _httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("YouTube API search error: {StatusCode} - {Content}", 
                    response.StatusCode, errorContent);
                return new List<YouTubeVideoDto>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var searchResponse = JsonConvert.DeserializeObject<YouTubeSearchResponse>(content);

            if (searchResponse?.Items == null)
            {
                return new List<YouTubeVideoDto>();
            }

            // Cache for 1 hour
            _cache.Set(cacheKey, searchResponse.Items, TimeSpan.FromHours(1));

            return searchResponse.Items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching YouTube videos for query: {Query}", query);
            return new List<YouTubeVideoDto>();
        }
    }

    public async Task<List<YouTubeVideoDto>> SearchTrackAsync(Track track, TransferOptions options)
    {
        try
        {
            var searchQueries = GenerateSearchQueries(track, options);
            var allResults = new List<YouTubeVideoDto>();

            foreach (var query in searchQueries)
            {
                var results = await SearchVideosAsync(query, options.SearchResultLimit);
                allResults.AddRange(results);

                // Rate limiting
                await Task.Delay(200);
            }

            // Remove duplicates and sort by relevance
            var uniqueResults = allResults
                .GroupBy(v => v.Id.VideoId)
                .Select(g => g.First())
                .ToList();

            return uniqueResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching track: {TrackName} by {Artist}", 
                track.Name, string.Join(", ", track.Artists.Select(a => a.Name)));
            return new List<YouTubeVideoDto>();
        }
    }

    public async Task<YouTubePlaylistResponse> CreatePlaylistAsync(string title, string description, bool isPublic, string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var request = new YouTubePlaylistCreateRequest
            {
                Snippet = new YouTubePlaylistSnippet
                {
                    Title = title,
                    Description = description
                },
                Status = new YouTubePlaylistStatus
                {
                    PrivacyStatus = isPublic ? "public" : "private"
                }
            };

            var jsonContent = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"{BaseUrl}/playlists?part=snippet,status";
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("YouTube API playlist creation error: {StatusCode} - {Content}", 
                    response.StatusCode, errorContent);
                throw new HttpRequestException($"YouTube API error: {response.StatusCode}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var playlist = JsonConvert.DeserializeObject<YouTubePlaylistResponse>(responseContent);

            if (playlist == null)
            {
                throw new InvalidOperationException("Failed to deserialize YouTube playlist response");
            }

            _logger.LogInformation("Created YouTube playlist: {PlaylistId} - {Title}", playlist.Id, title);
            return playlist;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating YouTube playlist: {Title}", title);
            throw;
        }
    }

    public async Task<YouTubePlaylistItemResponse?> AddVideoToPlaylistAsync(string playlistId, string videoId, string accessToken, int? position = null)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var request = new YouTubePlaylistItemCreateRequest
            {
                Snippet = new YouTubePlaylistItemSnippet
                {
                    PlaylistId = playlistId,
                    ResourceId = new YouTubeResourceId
                    {
                        VideoId = videoId
                    },
                    Position = position
                }
            };

            var jsonContent = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var url = $"{BaseUrl}/playlistItems?part=snippet";
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("YouTube API add video error: {StatusCode} - {Content}", 
                    response.StatusCode, errorContent);
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var playlistItem = JsonConvert.DeserializeObject<YouTubePlaylistItemResponse>(responseContent);

            return playlistItem;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding video {VideoId} to playlist {PlaylistId}", videoId, playlistId);
            return null;
        }
    }

    public async Task<bool> ValidateAccessTokenAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync($"{BaseUrl}/channels?part=snippet&mine=true");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating YouTube access token");
            return false;
        }
    }

    public int ParseDurationToMilliseconds(string isoDuration)
    {
        try
        {
            if (string.IsNullOrEmpty(isoDuration)) return 0;

            // Parse ISO 8601 duration (PT4M33S)
            var regex = new Regex(@"PT(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?");
            var match = regex.Match(isoDuration);

            if (!match.Success) return 0;

            var hours = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 0;
            var minutes = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 0;
            var seconds = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0;

            return (hours * 3600 + minutes * 60 + seconds) * 1000;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing duration: {Duration}", isoDuration);
            return 0;
        }
    }

    public (string title, string artist) ExtractTitleAndArtist(string videoTitle)
    {
        try
        {
            // Common patterns for music videos
            var patterns = new[]
            {
                @"^(.*?)\s*-\s*(.*?)(?:\s*\(.*\))?(?:\s*\[.*\])?$", // Artist - Title
                @"^(.*?)\s*by\s*(.*?)(?:\s*\(.*\))?(?:\s*\[.*\])?$", // Title by Artist
                @"^(.*?)\s*\|\s*(.*?)(?:\s*\(.*\))?(?:\s*\[.*\])?$"  // Artist | Title
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(videoTitle, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return (match.Groups[2].Value.Trim(), match.Groups[1].Value.Trim());
                }
            }

            // Fallback: return original title as both title and artist
            return (videoTitle, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error extracting title and artist from: {VideoTitle}", videoTitle);
            return (videoTitle, string.Empty);
        }
    }

    public double CalculateMatchScore(Track spotifyTrack, YouTubeVideoDto youtubeVideo)
    {
        try
        {
            var (videoTitle, videoArtist) = ExtractTitleAndArtist(youtubeVideo.Snippet.Title);
            
            // Calculate title similarity
            var titleScore = CalculateStringSimilarity(spotifyTrack.Name, videoTitle);
            
            // Calculate artist similarity
            var artistScore = 0.0;
            if (spotifyTrack.Artists.Any())
            {
                var spotifyArtists = string.Join(" ", spotifyTrack.Artists.Select(a => a.Name));
                artistScore = Math.Max(
                    CalculateStringSimilarity(spotifyArtists, videoArtist),
                    CalculateStringSimilarity(spotifyArtists, youtubeVideo.Snippet.ChannelTitle)
                );
            }

            // Duration similarity (if available)
            var durationScore = 1.0;
            if (!string.IsNullOrEmpty(youtubeVideo.ContentDetails?.Duration))
            {
                var youtubeDuration = ParseDurationToMilliseconds(youtubeVideo.ContentDetails.Duration);
                if (youtubeDuration > 0 && spotifyTrack.DurationMs > 0)
                {
                    var durationDiff = Math.Abs(youtubeDuration - spotifyTrack.DurationMs);
                    var tolerance = spotifyTrack.DurationMs * 0.1; // 10% tolerance
                    durationScore = Math.Max(0, 1 - (durationDiff / tolerance));
                }
            }

            // Weighted score
            var totalScore = (titleScore * 0.5) + (artistScore * 0.4) + (durationScore * 0.1);
            
            return Math.Min(1.0, totalScore);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error calculating match score");
            return 0.0;
        }
    }

    public YouTubeVideoDto? FindBestMatch(Track track, List<YouTubeVideoDto> searchResults, double minConfidence = 0.7)
    {
        if (!searchResults.Any()) return null;

        var scoredResults = searchResults
            .Select(video => new { Video = video, Score = CalculateMatchScore(track, video) })
            .Where(x => x.Score >= minConfidence)
            .OrderByDescending(x => x.Score)
            .ToList();

        return scoredResults.FirstOrDefault()?.Video;
    }

    public string GetPlaylistUrl(string playlistId)
    {
        return $"https://www.youtube.com/playlist?list={playlistId}";
    }

    public async Task<YouTubeVideoDto?> GetVideoDetailsAsync(string videoId)
    {
        try
        {
            var apiKey = _configuration["YouTube:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("YouTube API key not configured");
            }

            var cacheKey = $"youtube_video_{videoId}";
            if (_cache.TryGetValue(cacheKey, out YouTubeVideoDto? cachedVideo))
            {
                return cachedVideo;
            }

            var url = $"{BaseUrl}/videos?part=snippet,contentDetails&id={videoId}&key={apiKey}";
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var videoResponse = JsonConvert.DeserializeObject<YouTubeSearchResponse>(content);
            
            var video = videoResponse?.Items?.FirstOrDefault();
            if (video != null)
            {
                _cache.Set(cacheKey, video, TimeSpan.FromHours(2));
            }

            return video;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting video details for {VideoId}", videoId);
            return null;
        }
    }

    public async Task<YouTubeChannelDto?> GetUserChannelAsync(string accessToken)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

            var response = await _httpClient.GetAsync($"{BaseUrl}/channels?part=snippet&mine=true");
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var channelResponse = JsonConvert.DeserializeObject<dynamic>(content);
            
            // This is a simplified implementation - you'd need proper DTOs for channel response
            return new YouTubeChannelDto
            {
                Id = channelResponse?.items?[0]?.id ?? string.Empty,
                Title = channelResponse?.items?[0]?.snippet?.title ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting YouTube channel");
            return null;
        }
    }

    private List<string> GenerateSearchQueries(Track track, TransferOptions options)
    {
        var queries = new List<string>();
        var trackName = CleanSearchString(track.Name);
        var primaryArtist = track.Artists.FirstOrDefault()?.Name ?? string.Empty;
        var cleanArtist = CleanSearchString(primaryArtist);

        // Primary search with artist and track
        if (options.UseArtistInSearch && !string.IsNullOrEmpty(cleanArtist))
        {
            queries.Add($"{cleanArtist} {trackName}");
            queries.Add($"{trackName} {cleanArtist}");
        }

        // Search with album if enabled
        if (options.UseAlbumInSearch && !string.IsNullOrEmpty(track.Album))
        {
            var cleanAlbum = CleanSearchString(track.Album);
            queries.Add($"{cleanArtist} {trackName} {cleanAlbum}");
        }

        // Fallback: just track name
        queries.Add(trackName);

        return queries.Take(3).ToList(); // Limit to 3 queries to avoid hitting rate limits
    }

    private string CleanSearchString(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;

        // Remove common music-related suffixes/prefixes
        var cleaned = Regex.Replace(input, @"\(.*?\)|\[.*?\]", string.Empty); // Remove parentheses and brackets
        cleaned = Regex.Replace(cleaned, @"\b(feat|ft|featuring|with|vs|&)\b.*", string.Empty, RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"\s+", " "); // Normalize whitespace
        
        return cleaned.Trim();
    }

    private double CalculateStringSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return 0.0;

        str1 = str1.ToLowerInvariant();
        str2 = str2.ToLowerInvariant();

        if (str1 == str2) return 1.0;

        // Levenshtein distance based similarity
        var distance = ComputeLevenshteinDistance(str1, str2);
        var maxLength = Math.Max(str1.Length, str2.Length);
        
        return maxLength == 0 ? 1.0 : 1.0 - (double)distance / maxLength;
    }

    private int ComputeLevenshteinDistance(string str1, string str2)
    {
        var matrix = new int[str1.Length + 1, str2.Length + 1];

        for (int i = 0; i <= str1.Length; i++)
            matrix[i, 0] = i;

        for (int j = 0; j <= str2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= str1.Length; i++)
        {
            for (int j = 1; j <= str2.Length; j++)
            {
                var cost = str1[i - 1] == str2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[str1.Length, str2.Length];
    }
}