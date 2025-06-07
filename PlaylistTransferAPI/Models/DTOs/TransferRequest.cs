using System.ComponentModel.DataAnnotations;

namespace PlaylistTransferAPI.Models.DTOs;

public class TransferRequest
{
    [Required]
    public string SpotifyPlaylistId { get; set; } = string.Empty;
    
    [Required]
    public string SpotifyAccessToken { get; set; } = string.Empty;
    
    [Required]
    public string YouTubeAccessToken { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string NewPlaylistName { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string PlaylistDescription { get; set; } = string.Empty;
    
    public bool MakePublic { get; set; } = false;
    
    public string UserId { get; set; } = string.Empty;
    
    public TransferOptions Options { get; set; } = new();
}

public class TransferOptions
{
    public bool SkipDuplicates { get; set; } = true;
    
    public bool UseArtistInSearch { get; set; } = true;
    
    public bool UseAlbumInSearch { get; set; } = false;
    
    public int SearchResultLimit { get; set; } = 5;
    
    public double MinMatchConfidence { get; set; } = 0.7;
    
    public int MaxRetryAttempts { get; set; } = 3;
    
    public bool CreatePlaylistEvenIfEmpty { get; set; } = true;
}