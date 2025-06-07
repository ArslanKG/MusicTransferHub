using PlaylistTransferAPI.Models.DTOs;
using PlaylistTransferAPI.Models.Entities;

namespace PlaylistTransferAPI.Services.Interfaces;

public interface ISpotifyService
{
    /// <summary>
    /// Kullanıcının Spotify playlist'lerini getirir
    /// </summary>
    Task<List<SpotifyPlaylistDto>> GetUserPlaylistsAsync(string accessToken, int limit = 50, int offset = 0);

    /// <summary>
    /// Belirli bir playlist'in detaylarını getirir
    /// </summary>
    Task<SpotifyPlaylistDetailsDto?> GetPlaylistDetailsAsync(string playlistId, string accessToken);

    /// <summary>
    /// Playlist'teki tüm şarkıları getirir (sayfalandırma ile)
    /// </summary>
    Task<List<SpotifyTrackDto>> GetPlaylistTracksAsync(string playlistId, string accessToken);

    /// <summary>
    /// Spotify şarkı verisini Track entity'sine dönüştürür
    /// </summary>
    Track ConvertToTrack(SpotifyTrackDto spotifyTrack);

    /// <summary>
    /// Spotify playlist verisini Playlist entity'sine dönüştürür
    /// </summary>
    Playlist ConvertToPlaylist(SpotifyPlaylistDetailsDto spotifyPlaylist, List<Track> tracks);

    /// <summary>
    /// Access token'ın geçerliliğini kontrol eder
    /// </summary>
    Task<bool> ValidateAccessTokenAsync(string accessToken);

    /// <summary>
    /// Kullanıcı profilini getirir
    /// </summary>
    Task<SpotifyUserProfileDto?> GetUserProfileAsync(string accessToken);
}

public class SpotifyUserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public List<SpotifyImageDto> Images { get; set; } = new();
}