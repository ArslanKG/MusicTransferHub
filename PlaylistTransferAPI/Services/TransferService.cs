using PlaylistTransferAPI.Models.DTOs;
using PlaylistTransferAPI.Models.Entities;
using PlaylistTransferAPI.Services.Interfaces;

namespace PlaylistTransferAPI.Services;

public class TransferService : ITransferService
{
    private readonly ISpotifyService _spotifyService;
    private readonly IYouTubeService _youtubeService;
    private readonly ILogger<TransferService> _logger;

    public TransferService(
        ISpotifyService spotifyService,
        IYouTubeService youtubeService,
        ILogger<TransferService> logger)
    {
        _spotifyService = spotifyService;
        _youtubeService = youtubeService;
        _logger = logger;
    }

    public async Task<TransferResult> TransferPlaylistAsync(TransferRequest request)
    {
        var transferId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("Starting playlist transfer: {TransferId}", transferId);

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

            _logger.LogInformation("Found {TrackCount} tracks in Spotify playlist: {PlaylistName}", 
                convertedTracks.Count, playlistDetails.Name);

            var youtubePlaylist = await _youtubeService.CreatePlaylistAsync(
                request.NewPlaylistName,
                request.PlaylistDescription,
                request.MakePublic,
                request.YouTubeAccessToken);

            _logger.LogInformation("Created YouTube playlist: {PlaylistId}", youtubePlaylist.Id);

            var successfulTracks = 0;
            var failedTracks = new List<FailedTrackDto>();

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
                            
                            _logger.LogDebug("Successfully added track: {TrackName} by {Artist}", 
                                track.Name, string.Join(", ", track.Artists.Select(a => a.Name)));
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

                    await Task.Delay(500); // Rate limit protection
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error transferring track: {TrackName}", track.Name);
                    failedTracks.Add(CreateFailedTrack(track, ex.Message));
                }
            }

            var completedAt = DateTime.UtcNow;
            var duration = completedAt - startTime;

            _logger.LogInformation("Transfer completed: {TransferId}. Success: {Success}/{Total}, Failed: {Failed}",
                transferId, successfulTracks, convertedTracks.Count, failedTracks.Count);

            return new TransferResult
            {
                Success = true,
                TransferId = transferId,
                Message = $"Transfer tamamlandı. {successfulTracks}/{convertedTracks.Count} şarkı başarıyla aktarıldı.",
                YouTubePlaylistId = youtubePlaylist.Id,
                YouTubePlaylistUrl = _youtubeService.GetPlaylistUrl(youtubePlaylist.Id),
                Statistics = new TransferStatistics
                {
                    TotalTracks = convertedTracks.Count,
                    SuccessfulTracks = successfulTracks,
                    FailedTracks = failedTracks.Count,
                    OriginalPlaylistName = playlistDetails.Name,
                    NewPlaylistName = request.NewPlaylistName,
                    StartedAt = startTime,
                    CompletedAt = completedAt
                },
                FailedTracks = failedTracks,
                CompletedAt = completedAt,
                Duration = duration,
                Status = TransferStatus.Completed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during playlist transfer: {TransferId}", transferId);

            return new TransferResult
            {
                Success = false,
                TransferId = transferId,
                Message = "Transfer başarısız oldu: " + ex.Message,
                ErrorDetails = ex.Message,
                Status = TransferStatus.Failed,
                CompletedAt = DateTime.UtcNow,
                Duration = DateTime.UtcNow - startTime
            };
        }
    }

    public async Task<TransferResult?> GetTransferStatusAsync(string transferId)
    {
        await Task.CompletedTask;
        return null;
    }

    public async Task<List<TransferLog>> GetTransferHistoryAsync(string userId, int page = 1, int pageSize = 10)
    {
        await Task.CompletedTask;
        return new List<TransferLog>();
    }

    public async Task<bool> CancelTransferAsync(string transferId)
    {
        await Task.CompletedTask;
        return false;
    }

    public async Task<TransferResult> RetryTransferAsync(string transferId, TransferRequest? newRequest = null)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("Stateless service doesn't support retry. Please start a new transfer.");
    }

    public async Task<TransferStatistics> GetTransferStatisticsAsync(string? userId = null)
    {
        await Task.CompletedTask;
        return new TransferStatistics();
    }

    public async Task<int> GetActiveTransferCountAsync()
    {
        await Task.CompletedTask;
        return 0;
    }

    public async Task UpdateTransferProgressAsync(string transferId, int processedTracks, int successfulTracks, int failedTracks)
    {
        await Task.CompletedTask;
    }

    public async Task<TransferResult> RetryFailedTracksAsync(string transferId, string youtubeAccessToken)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("Stateless service doesn't support retry. Please start a new transfer.");
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