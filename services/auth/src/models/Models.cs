using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace service.src.models;

// --- Entity Models ---

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User
{
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    public required string PasswordHash { get; set; }

    // Navigation property: a user can have many playlists.
    public List<Playlist> Playlists { get; set; } = [];
}

/// <summary>
/// Represents a single song.
/// </summary>
public class Song
{
    public Guid Id { get; set; }

    [Required]
    public required string Title { get; set; }

    [Required]
    public required string Artist { get; set; }

    // This is the field for the BASE demo.
    public int PlayCount { get; set; } = 0;

    // Navigation property for the many-to-many relationship with Playlist.
    public List<PlaylistSong> PlaylistSongs { get; set; } = [];
}

/// <summary>
/// Represents a user-created playlist of songs.
/// </summary>
public class Playlist
{
    public Guid Id { get; set; }

    [Required]
    public required string Name { get; set; }

    // Foreign key to the User who owns this playlist.
    public Guid UserId { get; set; }
    public User? User { get; set; }

    // Navigation property for the many-to-many relationship with Song.
    public List<PlaylistSong> PlaylistSongs { get; set; } = [];
}

/// <summary>
/// This is the join table for the many-to-many relationship
/// between Playlists and Songs.
/// </summary>
public class PlaylistSong
{
    public Guid PlaylistId { get; set; }
    public Playlist? Playlist { get; set; }

    public Guid SongId { get; set; }
    public Song? Song { get; set; }
}


// --- Entity Framework DbContext ---

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    // Define the DbSets for all entities in your application.
    // Each service will interact with a subset of these tables.
    public DbSet<User> Users { get; set; }
    public DbSet<Song> Songs { get; set; }
    public DbSet<Playlist> Playlists { get; set; }
    public DbSet<PlaylistSong> PlaylistSongs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(u => u.Playlists)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId);

        modelBuilder.Entity<PlaylistSong>()
            .HasKey(ps => new { ps.PlaylistId, ps.SongId });

        modelBuilder.Entity<PlaylistSong>()
            .HasOne(ps => ps.Playlist)
            .WithMany(p => p.PlaylistSongs)
            .HasForeignKey(ps => ps.PlaylistId);

        modelBuilder.Entity<PlaylistSong>()
            .HasOne(ps => ps.Song)
            .WithMany(s => s.PlaylistSongs)
            .HasForeignKey(ps => ps.SongId);
    }
}
