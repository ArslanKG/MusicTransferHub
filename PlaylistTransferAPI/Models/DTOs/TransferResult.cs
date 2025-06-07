using PlaylistTransferAPI.Models.Entities;

namespace PlaylistTransferAPI.Models.DTOs;

public class TransferResult
{
    public bool Success { get; set; }
    
    public string TransferId { get; set; } = string.Empty;
    
    public string Message { get; set; } = string.Empty;
    
    public TransferStatistics Statistics { get; set; } = new();
    
    public string YouTubePlaylistId { get; set; } = string.Empty;
    
    public string YouTubePlaylistUrl { get; set; } = string.Empty;
    
    public List<FailedTrackDto> FailedTracks { get; set; } = new();
    
    public DateTime CompletedAt { get; set; }
    
    public TimeSpan Duration { get; set; }
    
    public TransferStatus Status { get; set; }
    
    public string ErrorDetails { get; set; } = string.Empty;
}

public class TransferStatistics
{
    public int TotalTracks { get; set; }
    
    public int SuccessfulTracks { get; set; }
    
    public int FailedTracks { get; set; }
    
    public int SkippedTracks { get; set; }
    
    public double SuccessRate => TotalTracks > 0 ? (double)SuccessfulTracks / TotalTracks * 100 : 0;
    
    public string OriginalPlaylistName { get; set; } = string.Empty;
    
    public string NewPlaylistName { get; set; } = string.Empty;
    
    public DateTime StartedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
}

public class FailedTrackDto
{
    public string TrackName { get; set; } = string.Empty;
    
    public string Artist { get; set; } = string.Empty;
    
    public string Album { get; set; } = string.Empty;
    
    public string FailureReason { get; set; } = string.Empty;
    
    public DateTime AttemptedAt { get; set; }
    
    public string SpotifyTrackId { get; set; } = string.Empty;
    
    public List<string> SearchAttempts { get; set; } = new();
}