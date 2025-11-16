using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using service.src.common.models;
using service.src.songs.services;

namespace service.src.songs.controllers;

[ApiController]
[Route("songs")]
[Produces("application/json")]
public class SongsController(ApplicationDbContext context, SongCacheService songCache) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SongDtoWithPlayCount>), StatusCodes.Status200OK)]
    public IActionResult GetAll()
    {
        var songs = songCache.GetAllSongs();
        return Ok(songs);
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(SongDtoWithPlayCount), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSongRequest request)
    {
        var newSong = new Song
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Artist = request.Artist,
        };

        context.Songs.Add(newSong);
        await context.SaveChangesAsync();

        var songDto = songCache.AddSong(newSong);

        return CreatedAtAction(nameof(GetAll), new { id = songDto.Id }, songDto);
    }

    [HttpPost("{songId:guid}/play")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SongDtoWithPlayCount), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IncrementPlayCount([FromRoute] Guid songId)
    {
        var updatedSong = await songCache.IncrementPlayCountAsync(songId);

        if (updatedSong is null)
        {
            return NotFound("Song not found in cache.");
        }

        return Ok(updatedSong);
    }
}

// --- DTOs ---

public record CreateSongRequest
{
    [Required]
    [MinLength(1)]
    public required string Title { get; init; }

    [Required]
    [MinLength(1)]
    public required string Artist { get; init; }
}

public record SongDtoWithPlayCount
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Artist { get; init; }
    public int PlayCount { get; init; }
}
