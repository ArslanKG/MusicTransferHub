using PlaylistTransferAPI.Models.DTOs;
using PlaylistTransferAPI.Models.Entities;

namespace PlaylistTransferAPI.Services.Interfaces;

public interface ITransferService
{
    /// <summary>
    /// Spotify playlist'ini YouTube Music'e transfer eder
    /// </summary>
    Task<TransferResult> TransferPlaylistAsync(TransferRequest request);

    /// <summary>
    /// Transfer durumunu getirir
    /// </summary>
    Task<TransferResult?> GetTransferStatusAsync(string transferId);

    /// <summary>
    /// Kullanıcının transfer geçmişini getirir
    /// </summary>
    Task<List<TransferLog>> GetTransferHistoryAsync(string userId, int page = 1, int pageSize = 10);

    /// <summary>
    /// Transfer işlemini iptal eder
    /// </summary>
    Task<bool> CancelTransferAsync(string transferId);

    /// <summary>
    /// Transfer'i yeniden başlatır (başarısız olan)
    /// </summary>
    Task<TransferResult> RetryTransferAsync(string transferId, TransferRequest? newRequest = null);

    /// <summary>
    /// Transfer istatistiklerini getirir
    /// </summary>
    Task<TransferStatistics> GetTransferStatisticsAsync(string? userId = null);

    /// <summary>
    /// Aktif transfer sayısını getirir
    /// </summary>
    Task<int> GetActiveTransferCountAsync();

    /// <summary>
    /// Transfer detaylarını günceller
    /// </summary>
    Task UpdateTransferProgressAsync(string transferId, int processedTracks, int successfulTracks, int failedTracks);

    /// <summary>
    /// Başarısız şarkıları yeniden dener
    /// </summary>
    Task<TransferResult> RetryFailedTracksAsync(string transferId, string youtubeAccessToken);
}