using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using PlaylistTransferAPI.Models.DTOs;
using PlaylistTransferAPI.Models.Responses;
using PlaylistTransferAPI.Services.Interfaces;
using System.Collections;
using System.Reflection;

namespace PlaylistTransferAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlaylistController : ControllerBase
{
    private readonly ISpotifyService _spotifyService;
    private readonly IYouTubeService _youtubeService;
    private readonly ITransferService _transferService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<PlaylistController> _logger;

    public PlaylistController(
        ISpotifyService spotifyService,
        IYouTubeService youtubeService,
        ITransferService transferService,
        IMemoryCache cache,
        ILogger<PlaylistController> logger)
    {
        _spotifyService = spotifyService;
        _youtubeService = youtubeService;
        _transferService = transferService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Kullanıcının Spotify playlist'lerini getirir
    /// </summary>
    [HttpGet("spotify/user-playlists")]
    public async Task<ActionResult<ApiResponse<List<SpotifyPlaylistDto>>>> GetUserPlaylists(
        [FromHeader] string spotifyAccessToken,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        try
        {
            if (string.IsNullOrEmpty(spotifyAccessToken))
            {
                return BadRequest(ApiResponse<List<SpotifyPlaylistDto>>.ErrorResponse("Spotify access token is required"));
            }

            var playlists = await _spotifyService.GetUserPlaylistsAsync(spotifyAccessToken, limit, offset);
            return Ok(ApiResponse<List<SpotifyPlaylistDto>>.SuccessResponse(playlists));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user playlists");
            return StatusCode(500, ApiResponse<List<SpotifyPlaylistDto>>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Belirli bir Spotify playlist'inin detaylarını getirir
    /// </summary>
    [HttpGet("spotify/{playlistId}")]
    public async Task<ActionResult<ApiResponse<SpotifyPlaylistDetailsDto>>> GetPlaylistDetails(
        string playlistId,
        [FromHeader] string spotifyAccessToken)
    {
        try
        {
            if (string.IsNullOrEmpty(spotifyAccessToken))
            {
                return BadRequest(ApiResponse<SpotifyPlaylistDetailsDto>.ErrorResponse("Spotify access token is required"));
            }

            var playlist = await _spotifyService.GetPlaylistDetailsAsync(playlistId, spotifyAccessToken);
            if (playlist == null)
            {
                return NotFound(ApiResponse<SpotifyPlaylistDetailsDto>.ErrorResponse("Playlist not found"));
            }

            return Ok(ApiResponse<SpotifyPlaylistDetailsDto>.SuccessResponse(playlist));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting playlist details for {PlaylistId}", playlistId);
            return StatusCode(500, ApiResponse<SpotifyPlaylistDetailsDto>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Spotify playlist'ini YouTube Music'e transfer eder
    /// </summary>
    [HttpPost("transfer")]
    public async Task<ActionResult<ApiResponse<TransferResult>>> TransferPlaylist([FromBody] TransferRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponse<TransferResult>.ErrorResponse("Validation failed", errors));
            }

            var result = await _transferService.TransferPlaylistAsync(request);
            
            if (result.Success)
            {
                return Ok(ApiResponse<TransferResult>.SuccessResponse(result, "Transfer started successfully"));
            }
            else
            {
                return BadRequest(ApiResponse<TransferResult>.ErrorResponse(result.Message, result.ErrorDetails));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring playlist");
            return StatusCode(500, ApiResponse<TransferResult>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Transfer durumunu kontrol eder
    /// </summary>
    [HttpGet("transfer-status/{transferId}")]
    public async Task<ActionResult<ApiResponse<TransferResult>>> GetTransferStatus(string transferId)
    {
        try
        {
            var result = await _transferService.GetTransferStatusAsync(transferId);
            if (result == null)
            {
                return NotFound(ApiResponse<TransferResult>.ErrorResponse("Transfer not found"));
            }

            return Ok(ApiResponse<TransferResult>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transfer status for {TransferId}", transferId);
            return StatusCode(500, ApiResponse<TransferResult>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Kullanıcının transfer geçmişini getirir
    /// </summary>
    [HttpGet("transfer-history")]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<Models.Entities.TransferLog>>>> GetTransferHistory(
        [FromQuery] string userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest(ApiResponse<PaginatedResponse<Models.Entities.TransferLog>>.ErrorResponse("User ID is required"));
            }

            var transfers = await _transferService.GetTransferHistoryAsync(userId, page, pageSize);
            var response = new PaginatedResponse<Models.Entities.TransferLog>
            {
                Items = transfers,
                PageNumber = page,
                PageSize = pageSize,
                TotalCount = transfers.Count
            };

            return Ok(ApiResponse<PaginatedResponse<Models.Entities.TransferLog>>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transfer history for user {UserId}", userId);
            return StatusCode(500, ApiResponse<PaginatedResponse<Models.Entities.TransferLog>>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Transfer işlemini iptal eder
    /// </summary>
    [HttpPost("cancel-transfer/{transferId}")]
    public async Task<ActionResult<ApiResponse<bool>>> CancelTransfer(string transferId)
    {
        try
        {
            var result = await _transferService.CancelTransferAsync(transferId);
            if (result)
            {
                return Ok(ApiResponse<bool>.SuccessResponse(true, "Transfer cancelled successfully"));
            }
            else
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse("Transfer could not be cancelled"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling transfer {TransferId}", transferId);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse("Internal server error"));
        }
    }

    /// <summary>
    /// Cache'i tamamen temizler
    /// </summary>
    [HttpGet("cache/clear")]
    [HttpPost("cache/clear")]
    public ActionResult<ApiResponse<bool>> ClearAllCache()
    {
        try
        {
            if (_cache is MemoryCache memoryCache)
            {
                var field = typeof(MemoryCache).GetField("_coherentState", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    var coherentState = field.GetValue(memoryCache);
                    var entriesCollection = coherentState?.GetType().GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (entriesCollection?.GetValue(coherentState) is IDictionary dictionary)
                    {
                        dictionary.Clear();
                        _logger.LogInformation("Cache cleared successfully");
                        return Ok(ApiResponse<bool>.SuccessResponse(true, "Cache başarıyla temizlendi"));
                    }
                }
                
                var clearMethod = typeof(MemoryCache).GetMethod("Clear", BindingFlags.Public | BindingFlags.Instance);
                if (clearMethod != null)
                {
                    clearMethod.Invoke(memoryCache, null);
                    _logger.LogInformation("Cache cleared successfully");
                    return Ok(ApiResponse<bool>.SuccessResponse(true, "Cache başarıyla temizlendi"));
                }
                
                return Ok(ApiResponse<bool>.SuccessResponse(false, "Cache temizleme metodu bulunamadı"));
            }
            else
            {
                return Ok(ApiResponse<bool>.SuccessResponse(false, "Cache temizlenemedi"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache temizleme sırasında hata oluştu: {Message}", ex.Message);
            return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Cache temizleme sırasında hata oluştu: {ex.Message}"));
        }
    }
}