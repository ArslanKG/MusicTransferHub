using System.ComponentModel.DataAnnotations;

namespace PlaylistTransferAPI.Models.Entities;

public class TransferLog
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public string SpotifyPlaylistId { get; set; } = string.Empty;
    
    public string YouTubePlaylistId { get; set; } = string.Empty;
    
    public string YouTubePlaylistUrl { get; set; } = string.Empty;
    
    [Required]
    public TransferStatus Status { get; set; } = TransferStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? CompletedAt { get; set; }
    
    public int TotalTracks { get; set; }
    
    public int SuccessfulTracks { get; set; }
    
    public int FailedTracks { get; set; }
    
    public string ErrorMessage { get; set; } = string.Empty;
    
    public TimeSpan? Duration { get; set; }
    
    public string PlaylistName { get; set; } = string.Empty;
    
    public List<FailedTrack> FailedTracksList { get; set; } = new();
}

public class FailedTrack
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string TransferLogId { get; set; } = string.Empty;
    
    [Required]
    public string TrackName { get; set; } = string.Empty;
    
    public string Artist { get; set; } = string.Empty;
    
    public string Album { get; set; } = string.Empty;
    
    [Required]
    public string FailureReason { get; set; } = string.Empty;
    
    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    
    public string SpotifyTrackId { get; set; } = string.Empty;
}

public enum TransferStatus
{
    Pending,
    InProgress,
    Completed,
    Failed,
    Cancelled
}