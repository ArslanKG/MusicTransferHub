using Newtonsoft.Json;

namespace PlaylistTransferAPI.Models.DTOs;

public class YouTubeSearchResponse
{
    [JsonProperty("items")]
    public List<YouTubeVideoDto> Items { get; set; } = new();
    
    [JsonProperty("pageInfo")]
    public YouTubePageInfo PageInfo { get; set; } = new();
    
    [JsonProperty("nextPageToken")]
    public string NextPageToken { get; set; } = string.Empty;
}

public class YouTubeVideoDto
{
    [JsonProperty("id")]
    public YouTubeVideoId Id { get; set; } = new();
    
    [JsonProperty("snippet")]
    public YouTubeVideoSnippet Snippet { get; set; } = new();
    
    [JsonProperty("contentDetails")]
    public YouTubeContentDetails ContentDetails { get; set; } = new();
}

public class YouTubeVideoId
{
    [JsonProperty("videoId")]
    public string VideoId { get; set; } = string.Empty;
}

public class YouTubeVideoSnippet
{
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonProperty("channelTitle")]
    public string ChannelTitle { get; set; } = string.Empty;
    
    [JsonProperty("publishedAt")]
    public DateTime PublishedAt { get; set; }
    
    [JsonProperty("thumbnails")]
    public YouTubeThumbnails Thumbnails { get; set; } = new();
}

public class YouTubeContentDetails
{
    [JsonProperty("duration")]
    public string Duration { get; set; } = string.Empty;
}

public class YouTubeThumbnails
{
    [JsonProperty("default")]
    public YouTubeThumbnail Default { get; set; } = new();
    
    [JsonProperty("medium")]
    public YouTubeThumbnail Medium { get; set; } = new();
    
    [JsonProperty("high")]
    public YouTubeThumbnail High { get; set; } = new();
}

public class YouTubeThumbnail
{
    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonProperty("width")]
    public int Width { get; set; }
    
    [JsonProperty("height")]
    public int Height { get; set; }
}

public class YouTubePageInfo
{
    [JsonProperty("totalResults")]
    public int TotalResults { get; set; }
    
    [JsonProperty("resultsPerPage")]
    public int ResultsPerPage { get; set; }
}

// Playlist creation DTOs
public class YouTubePlaylistCreateRequest
{
    [JsonProperty("snippet")]
    public YouTubePlaylistSnippet Snippet { get; set; } = new();
    
    [JsonProperty("status")]
    public YouTubePlaylistStatus Status { get; set; } = new();
}

public class YouTubePlaylistSnippet
{
    [JsonProperty("title")]
    public string Title { get; set; } = string.Empty;
    
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonProperty("defaultLanguage")]
    public string DefaultLanguage { get; set; } = "en";
}

public class YouTubePlaylistStatus
{
    [JsonProperty("privacyStatus")]
    public string PrivacyStatus { get; set; } = "private";
}

public class YouTubePlaylistResponse
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("snippet")]
    public YouTubePlaylistSnippet Snippet { get; set; } = new();
    
    [JsonProperty("status")]
    public YouTubePlaylistStatus Status { get; set; } = new();
}

// Playlist item DTOs
public class YouTubePlaylistItemCreateRequest
{
    [JsonProperty("snippet")]
    public YouTubePlaylistItemSnippet Snippet { get; set; } = new();
}

public class YouTubePlaylistItemSnippet
{
    [JsonProperty("playlistId")]
    public string PlaylistId { get; set; } = string.Empty;
    
    [JsonProperty("resourceId")]
    public YouTubeResourceId ResourceId { get; set; } = new();
    
    [JsonProperty("position")]
    public int? Position { get; set; }
}

public class YouTubeResourceId
{
    [JsonProperty("kind")]
    public string Kind { get; set; } = "youtube#video";
    
    [JsonProperty("videoId")]
    public string VideoId { get; set; } = string.Empty;
}

public class YouTubePlaylistItemResponse
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("snippet")]
    public YouTubePlaylistItemSnippet Snippet { get; set; } = new();
}

// Error response
public class YouTubeErrorResponse
{
    [JsonProperty("error")]
    public YouTubeError Error { get; set; } = new();
}

public class YouTubeError
{
    [JsonProperty("code")]
    public int Code { get; set; }
    
    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonProperty("errors")]
    public List<YouTubeErrorDetail> Errors { get; set; } = new();
}

public class YouTubeErrorDetail
{
    [JsonProperty("domain")]
    public string Domain { get; set; } = string.Empty;
    
    [JsonProperty("reason")]
    public string Reason { get; set; } = string.Empty;
    
    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;
}