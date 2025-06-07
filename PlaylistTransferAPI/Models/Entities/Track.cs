using System.ComponentModel.DataAnnotations;

namespace PlaylistTransferAPI.Models.Entities;

public class Track
{
    [Key]
    public string Id { get; set; } = string.Empty;
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public List<Artist> Artists { get; set; } = new();
    
    public string Album { get; set; } = string.Empty;
    
    public int DurationMs { get; set; }
    
    public string SpotifyId { get; set; } = string.Empty;
    
    public string YouTubeVideoId { get; set; } = string.Empty;
    
    public bool IsMatched { get; set; }
    
    public double MatchConfidence { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Artist
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SpotifyId { get; set; } = string.Empty;
}