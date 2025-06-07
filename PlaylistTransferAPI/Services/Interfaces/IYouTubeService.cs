using PlaylistTransferAPI.Models.DTOs;
using PlaylistTransferAPI.Models.Entities;

namespace PlaylistTransferAPI.Services.Interfaces;

public interface IYouTubeService
{
    /// <summary>
    /// YouTube'da şarkı arar
    /// </summary>
    Task<List<YouTubeVideoDto>> SearchVideosAsync(string query, int maxResults = 5);

    /// <summary>
    /// Şarkı bilgilerini kullanarak akıllı arama yapar
    /// </summary>
    Task<List<YouTubeVideoDto>> SearchTrackAsync(Track track, TransferOptions options);

    /// <summary>
    /// YouTube'da yeni playlist oluşturur
    /// </summary>
    Task<YouTubePlaylistResponse> CreatePlaylistAsync(string title, string description, bool isPublic, string accessToken);

    /// <summary>
    /// Playlist'e video ekler
    /// </summary>
    Task<YouTubePlaylistItemResponse?> AddVideoToPlaylistAsync(string playlistId, string videoId, string accessToken, int? position = null);

    /// <summary>
    /// Access token'ın geçerliliğini kontrol eder
    /// </summary>
    Task<bool> ValidateAccessTokenAsync(string accessToken);

    /// <summary>
    /// Video süresini milisaniyeye çevirir (PT4M33S formatından)
    /// </summary>
    int ParseDurationToMilliseconds(string isoDuration);

    /// <summary>
    /// Video başlığından şarkı adı ve sanatçı bilgisini çıkarır
    /// </summary>
    (string title, string artist) ExtractTitleAndArtist(string videoTitle);

    /// <summary>
    /// İki şarkı arasındaki benzerlik skorunu hesaplar
    /// </summary>
    double CalculateMatchScore(Track spotifyTrack, YouTubeVideoDto youtubeVideo);

    /// <summary>
    /// En iyi eşleşen videoyu bulur
    /// </summary>
    YouTubeVideoDto? FindBestMatch(Track track, List<YouTubeVideoDto> searchResults, double minConfidence = 0.7);

    /// <summary>
    /// Playlist'in URL'sini oluşturur
    /// </summary>
    string GetPlaylistUrl(string playlistId);

    /// <summary>
    /// Video detaylarını getirir (süre bilgisi dahil)
    /// </summary>
    Task<YouTubeVideoDto?> GetVideoDetailsAsync(string videoId);

    /// <summary>
    /// Kullanıcının YouTube kanalını getirir
    /// </summary>
    Task<YouTubeChannelDto?> GetUserChannelAsync(string accessToken);
}

public class YouTubeChannelDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CustomUrl { get; set; } = string.Empty;
    public YouTubeThumbnails Thumbnails { get; set; } = new();
}