using Microsoft.EntityFrameworkCore;
using PlaylistTransferAPI.Data;
using PlaylistTransferAPI.Models.DTOs;
using PlaylistTransferAPI.Models.Entities;
using PlaylistTransferAPI.Services.Interfaces;

namespace PlaylistTransferAPI.Services;

public class TransferService : ITransferService
{
    private readonly ISpotifyService _spotifyService;
    private readonly IYouTubeService _youtubeService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TransferService> _logger;

    public TransferService(
        ISpotifyService spotifyService,
        IYouTubeService youtubeService,
        ApplicationDbContext context,
        ILogger<TransferService> logger)
    {
        _spotifyService = spotifyService;
        _youtubeService = youtubeService;
        _context = context;
        _logger = logger;
    }

    public async Task<TransferResult> TransferPlaylistAsync(TransferRequest request)
    {
        var transferId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;

        try
        {
            var spotifyValid = await _spotifyService.ValidateAccessTokenAsync(request.SpotifyAccessToken);
            var youtubeValid = await _youtubeService.ValidateAccessTokenAsync(request.YouTubeAccessToken);

            if (!spotifyValid)
            {
                return new TransferResult
                {
                    Success = false,
                    TransferId = transferId,
                    Message = "Spotify token geçersiz. Lütfen tekrar giriş yapın.",
                    Status = TransferStatus.Failed
                };
            }
            
            if (!youtubeValid)
            {
                return new TransferResult
                {
                    Success = false,
                    TransferId = transferId,
                    Message = "YouTube token bulunamadı. Lütfen YouTube ile giriş yapın.",
                    Status = TransferStatus.Failed
                };
            }

            // Get playlist from Spotify
            var playlistDetails = await _spotifyService.GetPlaylistDetailsAsync(request.SpotifyPlaylistId, request.SpotifyAccessToken);
            if (playlistDetails == null)
            {
                return new TransferResult
                {
                    Success = false,
                    TransferId = transferId,
                    Message = "Playlist not found on Spotify",
                    Status = TransferStatus.Failed
                };
            }

            var tracks = await _spotifyService.GetPlaylistTracksAsync(request.SpotifyPlaylistId, request.SpotifyAccessToken);
            var convertedTracks = tracks.Select(_spotifyService.ConvertToTrack).ToList();

            // Create transfer log
            var transferLog = new TransferLog
            {
                Id = transferId,
                UserId = request.UserId,
                SpotifyPlaylistId = request.SpotifyPlaylistId,
                Status = TransferStatus.InProgress,
                TotalTracks = convertedTracks.Count,
                PlaylistName = request.NewPlaylistName,
                CreatedAt = startTime
            };

            _context.TransferLogs.Add(transferLog);
            await _context.SaveChangesAsync();

            // Create YouTube playlist
            var youtubePlaylist = await _youtubeService.CreatePlaylistAsync(
                request.NewPlaylistName,
                request.PlaylistDescription,
                request.MakePublic,
                request.YouTubeAccessToken);

            transferLog.YouTubePlaylistId = youtubePlaylist.Id;
            transferLog.YouTubePlaylistUrl = _youtubeService.GetPlaylistUrl(youtubePlaylist.Id);

            var successfulTracks = 0;
            var failedTracks = new List<FailedTrackDto>();

            // Transfer tracks
            foreach (var track in convertedTracks)
            {
                try
                {
                    var searchResults = await _youtubeService.SearchTrackAsync(track, request.Options);
                    var bestMatch = _youtubeService.FindBestMatch(track, searchResults, request.Options.MinMatchConfidence);

                    if (bestMatch != null)
                    {
                        var addResult = await _youtubeService.AddVideoToPlaylistAsync(
                            youtubePlaylist.Id,
                            bestMatch.Id.VideoId,
                            request.YouTubeAccessToken);

                        if (addResult != null)
                        {
                            successfulTracks++;
                            track.YouTubeVideoId = bestMatch.Id.VideoId;
                            track.IsMatched = true;
                            track.MatchConfidence = _youtubeService.CalculateMatchScore(track, bestMatch);
                        }
                        else
                        {
                            failedTracks.Add(CreateFailedTrack(track, "Failed to add to YouTube playlist"));
                        }
                    }
                    else
                    {
                        failedTracks.Add(CreateFailedTrack(track, "No suitable match found"));
                    }

                    await UpdateTransferProgressAsync(transferId, successfulTracks + failedTracks.Count, successfulTracks, failedTracks.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error transferring track: {TrackName}", track.Name);
                    failedTracks.Add(CreateFailedTrack(track, ex.Message));
                }
            }

            // Update final status
            transferLog.Status = TransferStatus.Completed;
            transferLog.CompletedAt = DateTime.UtcNow;
            transferLog.Duration = transferLog.CompletedAt - transferLog.CreatedAt;
            transferLog.SuccessfulTracks = successfulTracks;
            transferLog.FailedTracks = failedTracks.Count;
            
            // Save failed tracks to database
            if (failedTracks.Any())
            {
                transferLog.FailedTracksList = failedTracks.Select(ft => new FailedTrack
                {
                    TransferLogId = transferId,
                    TrackName = ft.TrackName,
                    Artist = ft.Artist,
                    Album = ft.Album,
                    FailureReason = ft.FailureReason,
                    AttemptedAt = ft.AttemptedAt,
                    SpotifyTrackId = ft.SpotifyTrackId
                }).ToList();
            }

            await _context.SaveChangesAsync();

            return new TransferResult
            {
                Success = true,
                TransferId = transferId,
                Message = "Transfer completed",
                YouTubePlaylistId = youtubePlaylist.Id,
                YouTubePlaylistUrl = transferLog.YouTubePlaylistUrl,
                Statistics = new TransferStatistics
                {
                    TotalTracks = convertedTracks.Count,
                    SuccessfulTracks = successfulTracks,
                    FailedTracks = failedTracks.Count,
                    OriginalPlaylistName = playlistDetails.Name,
                    NewPlaylistName = request.NewPlaylistName,
                    StartedAt = startTime,
                    CompletedAt = DateTime.UtcNow
                },
                FailedTracks = failedTracks,
                CompletedAt = DateTime.UtcNow,
                Duration = DateTime.UtcNow - startTime,
                Status = TransferStatus.Completed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during playlist transfer: {TransferId}", transferId);
            
            // Update transfer log with error
            var transferLog = await _context.TransferLogs.FindAsync(transferId);
            if (transferLog != null)
            {
                transferLog.Status = TransferStatus.Failed;
                transferLog.ErrorMessage = ex.Message;
                transferLog.CompletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return new TransferResult
            {
                Success = false,
                TransferId = transferId,
                Message = "Transfer failed",
                ErrorDetails = ex.Message,
                Status = TransferStatus.Failed
            };
        }
    }

    public async Task<TransferResult?> GetTransferStatusAsync(string transferId)
    {
        var transferLog = await _context.TransferLogs.FindAsync(transferId);
        if (transferLog == null) return null;

        // Get failed tracks from database if transfer is completed
        var failedTracks = new List<FailedTrackDto>();
        if (transferLog.Status == TransferStatus.Completed && transferLog.FailedTracksList != null && transferLog.FailedTracksList.Any())
        {
            failedTracks = transferLog.FailedTracksList.Select(ft => new FailedTrackDto
            {
                TrackName = ft.TrackName,
                Artist = ft.Artist,
                Album = ft.Album,
                FailureReason = ft.FailureReason,
                AttemptedAt = ft.AttemptedAt,
                SpotifyTrackId = ft.SpotifyTrackId
            }).ToList();
        }

        return new TransferResult
        {
            Success = transferLog.Status == TransferStatus.Completed,
            TransferId = transferLog.Id,
            Status = transferLog.Status,
            YouTubePlaylistId = transferLog.YouTubePlaylistId,
            YouTubePlaylistUrl = transferLog.YouTubePlaylistUrl,
            Statistics = new TransferStatistics
            {
                TotalTracks = transferLog.TotalTracks,
                SuccessfulTracks = transferLog.SuccessfulTracks,
                FailedTracks = transferLog.FailedTracks,
                NewPlaylistName = transferLog.PlaylistName,
                StartedAt = transferLog.CreatedAt,
                CompletedAt = transferLog.CompletedAt
            },
            FailedTracks = failedTracks,
            CompletedAt = transferLog.CompletedAt ?? DateTime.UtcNow,
            Duration = transferLog.Duration ?? TimeSpan.Zero
        };
    }

    public async Task<List<TransferLog>> GetTransferHistoryAsync(string userId, int page = 1, int pageSize = 10)
    {
        return await _context.TransferLogs
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<bool> CancelTransferAsync(string transferId)
    {
        var transferLog = await _context.TransferLogs.FindAsync(transferId);
        if (transferLog == null || transferLog.Status != TransferStatus.InProgress)
            return false;

        transferLog.Status = TransferStatus.Cancelled;
        transferLog.CompletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TransferResult> RetryTransferAsync(string transferId, TransferRequest? newRequest = null)
    {
        throw new NotImplementedException();
    }

    public async Task<TransferStatistics> GetTransferStatisticsAsync(string? userId = null)
    {
        var query = _context.TransferLogs.AsQueryable();
        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.UserId == userId);
        }

        var stats = await query
            .GroupBy(t => 1)
            .Select(g => new TransferStatistics
            {
                TotalTracks = g.Sum(t => t.TotalTracks),
                SuccessfulTracks = g.Sum(t => t.SuccessfulTracks),
                FailedTracks = g.Sum(t => t.FailedTracks)
            })
            .FirstOrDefaultAsync();

        return stats ?? new TransferStatistics();
    }

    public async Task<int> GetActiveTransferCountAsync()
    {
        return await _context.TransferLogs
            .CountAsync(t => t.Status == TransferStatus.InProgress);
    }

    public async Task UpdateTransferProgressAsync(string transferId, int processedTracks, int successfulTracks, int failedTracks)
    {
        var transferLog = await _context.TransferLogs.FindAsync(transferId);
        if (transferLog != null)
        {
            transferLog.SuccessfulTracks = successfulTracks;
            transferLog.FailedTracks = failedTracks;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<TransferResult> RetryFailedTracksAsync(string transferId, string youtubeAccessToken)
    {
        throw new NotImplementedException();
    }

    private FailedTrackDto CreateFailedTrack(Track track, string reason)
    {
        return new FailedTrackDto
        {
            TrackName = track.Name,
            Artist = string.Join(", ", track.Artists.Select(a => a.Name)),
            Album = track.Album,
            FailureReason = reason,
            AttemptedAt = DateTime.UtcNow,
            SpotifyTrackId = track.SpotifyId
        };
    }
}