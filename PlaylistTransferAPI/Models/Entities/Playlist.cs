using System.ComponentModel.DataAnnotations;

namespace PlaylistTransferAPI.Models.Entities;

public class Playlist
{
    [Key]
    public string Id { get; set; } = string.Empty;
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Owner { get; set; } = string.Empty;
    
    public List<Track> Tracks { get; set; } = new();
    
    public int TotalTracks { get; set; }
    
    public string ImageUrl { get; set; } = string.Empty;
    
    public bool IsPublic { get; set; }
    
    public string SpotifyId { get; set; } = string.Empty;
    
    public string YouTubeId { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}