using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using service.src.models;
using service.src.services;

namespace service.src.controllers;

[ApiController]
[Authorize]
[Produces("application/json")]
public class PlaylistsController(ApplicationDbContext context, JwtTokenService jwtTokenService) : Controller
{
    /// <summary>
    /// Gets all playlists owned by the authenticated user.
    /// </summary>
    [HttpGet("")]
    [ProducesResponseType(typeof(List<PlaylistDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        var userId = jwtTokenService.GetCurrentUser()!.Id;

        var playlists = await context
            .Playlists.Where(p => p.UserId == userId)
            .Select(p => new PlaylistDto // Projecting to a DTO
            {
                Id = p.Id,
                Name = p.Name,
                Songs = p
                    .PlaylistSongs.Select(ps => new SongDto
                    {
                        Id = ps.Song!.Id,
                        Title = ps.Song.Title,
                        Artist = ps.Song.Artist,
                    })
                    .ToList(),
            })
            .ToListAsync();

        return Ok(playlists);
    }

    /// <summary>
    /// Creates a new playlist for the authenticated user.
    /// </summary>
    [HttpPost("")]
    [ProducesResponseType(typeof(PlaylistDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Create([FromBody] CreatePlaylistRequest request)
    {
        var userId = jwtTokenService.GetCurrentUser()!.Id;

        var newPlaylist = new Playlist
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            UserId = userId,
        };

        context.Playlists.Add(newPlaylist);
        await context.SaveChangesAsync();

        var playlistDto = new PlaylistDto
        {
            Id = newPlaylist.Id,
            Name = newPlaylist.Name,
            Songs = [],
        };

        // Return a 201 Created status with a link to the new resource
        return CreatedAtAction(nameof(GetAll), new { id = newPlaylist.Id }, playlistDto);
    }

    /// <summary>
    /// Adds a song to a specific playlist owned by the authenticated user.
    /// </summary>
    [HttpPost("{playlistId:guid}/songs")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddSongToPlaylist(
        [FromRoute] Guid playlistId,
        [FromBody] AddSongRequest request
    )
    {
        var userId = jwtTokenService.GetCurrentUser()!.Id;

        // Find the playlist and include its current songs to check for duplicates.
        var playlist = await context
            .Playlists.Include(p => p.PlaylistSongs)
            .FirstOrDefaultAsync(p => p.Id == playlistId);

        if (playlist == null || playlist.UserId != userId)
        {
            // If the playlist doesn't exist or doesn't belong to the user, return NotFound.
            return NotFound("Playlist not found.");
        }

        // Check if the song itself exists.
        var songExists = await context.Songs.AnyAsync(s => s.Id == request.SongId);
        if (!songExists)
        {
            return NotFound("Song not found.");
        }

        // Check if the song is already in the playlist.
        var isAlreadyInPlaylist = playlist.PlaylistSongs.Any(ps => ps.SongId == request.SongId);
        if (isAlreadyInPlaylist)
        {
            return Conflict("Song is already in this playlist.");
        }

        var playlistSong = new PlaylistSong { PlaylistId = playlistId, SongId = request.SongId };

        context.PlaylistSongs.Add(playlistSong);
        await context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Deletes a specific playlist owned by the authenticated user.
    /// </summary>
    [HttpDelete("{playlistId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid playlistId)
    {
        var userId = jwtTokenService.GetCurrentUser()!.Id;

        var playlist = await context.Playlists.FindAsync(playlistId);

        if (playlist == null)
        {
            return NotFound();
        }

        if (playlist.UserId != userId)
        {
            return NotFound();
        }

        context.Playlists.Remove(playlist);
        await context.SaveChangesAsync();

        return NoContent();
    }
}

// --- DTOs (Data Transfer Objects) ---

/// <summary>
/// Data needed to create a new playlist.
/// </summary>
public record CreatePlaylistRequest
{
    [Required]
    [MinLength(1)]
    public required string Name { get; init; }
}

/// <summary>
/// A simplified representation of a song for client responses.
/// </summary>
public record SongDto
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Artist { get; init; }
}

/// <summary>
/// A representation of a playlist, including its songs, for client responses.
/// </summary>
public record PlaylistDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public List<SongDto> Songs { get; init; } = [];
}

/// <summary>
/// Data needed to add a song to a playlist.
/// </summary>
public record AddSongRequest
{
    [Required]
    public Guid SongId { get; init; }
}
