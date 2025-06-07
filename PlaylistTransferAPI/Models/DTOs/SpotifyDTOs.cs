using Newtonsoft.Json;

namespace PlaylistTransferAPI.Models.DTOs;

public class SpotifyPlaylistResponse
{
    [JsonProperty("items")]
    public List<SpotifyPlaylistDto> Items { get; set; } = new();
    
    [JsonProperty("total")]
    public int Total { get; set; }
    
    [JsonProperty("limit")]
    public int Limit { get; set; }
    
    [JsonProperty("offset")]
    public int Offset { get; set; }
}

public class SpotifyPlaylistDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonProperty("owner")]
    public SpotifyOwnerDto Owner { get; set; } = new();
    
    [JsonProperty("tracks")]
    public SpotifyTracksMetadata Tracks { get; set; } = new();
    
    [JsonProperty("images")]
    public List<SpotifyImageDto> Images { get; set; } = new();
    
    [JsonProperty("public")]
    public bool? Public { get; set; }
    
    [JsonProperty("collaborative")]
    public bool Collaborative { get; set; }
}

public class SpotifyPlaylistDetailsDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonProperty("owner")]
    public SpotifyOwnerDto Owner { get; set; } = new();
    
    [JsonProperty("tracks")]
    public SpotifyTracksResponse Tracks { get; set; } = new();
    
    [JsonProperty("images")]
    public List<SpotifyImageDto> Images { get; set; } = new();
    
    [JsonProperty("public")]
    public bool? Public { get; set; }
}

public class SpotifyTracksResponse
{
    [JsonProperty("items")]
    public List<SpotifyTrackItemDto> Items { get; set; } = new();
    
    [JsonProperty("total")]
    public int Total { get; set; }
    
    [JsonProperty("limit")]
    public int Limit { get; set; }
    
    [JsonProperty("offset")]
    public int Offset { get; set; }
    
    [JsonProperty("next")]
    public string? Next { get; set; }
}

public class SpotifyTrackItemDto
{
    [JsonProperty("track")]
    public SpotifyTrackDto Track { get; set; } = new();
    
    [JsonProperty("added_at")]
    public DateTime AddedAt { get; set; }
}

public class SpotifyTrackDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("artists")]
    public List<SpotifyArtistDto> Artists { get; set; } = new();
    
    [JsonProperty("album")]
    public SpotifyAlbumDto Album { get; set; } = new();
    
    [JsonProperty("duration_ms")]
    public int DurationMs { get; set; }
    
    [JsonProperty("external_ids")]
    public SpotifyExternalIds ExternalIds { get; set; } = new();
    
    [JsonProperty("popularity")]
    public int Popularity { get; set; }
}

public class SpotifyArtistDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("external_urls")]
    public SpotifyExternalUrls ExternalUrls { get; set; } = new();
}

public class SpotifyAlbumDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonProperty("release_date")]
    public string ReleaseDate { get; set; } = string.Empty;
    
    [JsonProperty("images")]
    public List<SpotifyImageDto> Images { get; set; } = new();
}

public class SpotifyOwnerDto
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonProperty("display_name")]
    public string DisplayName { get; set; } = string.Empty;
}

public class SpotifyImageDto
{
    [JsonProperty("url")]
    public string Url { get; set; } = string.Empty;
    
    [JsonProperty("height")]
    public int? Height { get; set; }
    
    [JsonProperty("width")]
    public int? Width { get; set; }
}

public class SpotifyTracksMetadata
{
    [JsonProperty("total")]
    public int Total { get; set; }
}

public class SpotifyExternalIds
{
    [JsonProperty("isrc")]
    public string Isrc { get; set; } = string.Empty;
}

public class SpotifyExternalUrls
{
    [JsonProperty("spotify")]
    public string Spotify { get; set; } = string.Empty;
}