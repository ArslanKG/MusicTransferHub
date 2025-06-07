using Microsoft.EntityFrameworkCore;
using PlaylistTransferAPI.Models.Entities;

namespace PlaylistTransferAPI.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<TransferLog> TransferLogs { get; set; }
    public DbSet<FailedTrack> FailedTracks { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<Playlist> Playlists { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TransferLog configuration
        modelBuilder.Entity<TransferLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SpotifyPlaylistId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.YouTubePlaylistId).HasMaxLength(50);
            entity.Property(e => e.PlaylistName).HasMaxLength(200);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.Status).HasConversion<string>();
            
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Status);
        });

        // FailedTrack configuration
        modelBuilder.Entity<FailedTrack>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.TrackName).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Artist).HasMaxLength(200);
            entity.Property(e => e.Album).HasMaxLength(200);
            entity.Property(e => e.FailureReason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.SpotifyTrackId).HasMaxLength(50);
            
            entity.HasIndex(e => e.TransferLogId);
        });

        // Track configuration
        modelBuilder.Entity<Track>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Album).HasMaxLength(200);
            entity.Property(e => e.SpotifyId).HasMaxLength(50);
            entity.Property(e => e.YouTubeVideoId).HasMaxLength(50);
            
            entity.HasIndex(e => e.SpotifyId);
            entity.HasIndex(e => e.YouTubeVideoId);
        });

        // Playlist configuration
        modelBuilder.Entity<Playlist>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Owner).HasMaxLength(100);
            entity.Property(e => e.SpotifyId).HasMaxLength(50);
            entity.Property(e => e.YouTubeId).HasMaxLength(50);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            
            entity.HasIndex(e => e.SpotifyId);
            entity.HasIndex(e => e.YouTubeId);
            entity.HasIndex(e => e.Owner);
        });

        // Artist configuration (for Track.Artists JSON conversion)
        modelBuilder.Entity<Track>()
            .Property(e => e.Artists)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Artist>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Artist>()
            );

        // Playlist Tracks relationship
        modelBuilder.Entity<Playlist>()
            .Property(e => e.Tracks)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<Track>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<Track>()
            );

        // TransferLog FailedTracksList relationship
        modelBuilder.Entity<TransferLog>()
            .Property(e => e.FailedTracksList)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<List<FailedTrack>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<FailedTrack>()
            );
    }
}