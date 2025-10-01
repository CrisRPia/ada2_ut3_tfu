using System.Collections.Concurrent;
using service.src.controllers;
using service.src.models;

namespace service.src.services;

public class SongCacheService(IServiceProvider serviceProvider)
{
    private readonly ConcurrentDictionary<Guid, SongDto> _songReadCache = new();
    private ConcurrentDictionary<Guid, int> _playCountDiffs = new();

    public void InitializeCache(IEnumerable<Song> songs)
    {
        foreach (var song in songs)
        {
            _songReadCache.TryAdd(song.Id, ToDto(song));
        }
    }

    public List<SongDto> GetAllSongs()
    {
        var allSongs = new List<SongDto>();

        foreach (var song in _songReadCache.Values)
        {
            var diff = _playCountDiffs.GetValueOrDefault(song.Id, 0);
            allSongs.Add(song with { PlayCount = song.PlayCount + diff });
        }

        return [.. allSongs.OrderBy(s => s.Title)];
    }

    public SongDto AddSong(Song song)
    {
        var dto = ToDto(song);
        _songReadCache.TryAdd(song.Id, dto);
        return dto;
    }

    public async Task<SongDto?> IncrementPlayCountAsync(Guid songId)
    {
        if (!_songReadCache.ContainsKey(songId))
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var songFromDb = await dbContext.Songs.FindAsync(songId);

            if (songFromDb != null)
            {
                AddSong(songFromDb);
            }
            else
            {
                return null;
            }
        }

        var newDiff = _playCountDiffs.AddOrUpdate(songId, 1, (key, currentDiff) => currentDiff + 1);
        var cachedSong = _songReadCache[songId];
        return cachedSong with { PlayCount = cachedSong.PlayCount + newDiff };
    }

    public int FullCacheRefresh(List<Song> songsFromDb)
    {
        int changeCount = 0;
        foreach (var song in songsFromDb)
        {
            var newDto = ToDto(song);
            if (_songReadCache.TryGetValue(song.Id, out var existingDto))
            {
                if (existingDto.PlayCount != newDto.PlayCount)
                {
                    _songReadCache.TryUpdate(song.Id, newDto, existingDto);
                    changeCount++;
                }
            }
            else
            {
                _songReadCache.TryAdd(song.Id, newDto);
                changeCount++;
            }
        }

        return changeCount;
    }

    public ConcurrentDictionary<Guid, int> GetAndResetDiffs()
    {
        return Interlocked.Exchange(ref _playCountDiffs, new ConcurrentDictionary<Guid, int>());
    }

    public bool UpdateReadCache(Song songFromDb)
    {
        if (_songReadCache.TryGetValue(songFromDb.Id, out var existingDto))
        {
            var newDto = ToDto(songFromDb);
            if (existingDto.PlayCount != newDto.PlayCount)
            {
                _songReadCache.TryUpdate(songFromDb.Id, newDto, existingDto);
                return true;
            }
        }
        return false;
    }

    public List<Guid> GetAllSongIds()
    {
        return [.. _songReadCache.Keys];
    }

    private static SongDto ToDto(Song song) =>
        new()
        {
            Id = song.Id,
            Title = song.Title,
            Artist = song.Artist,
            PlayCount = song.PlayCount,
        };
}
