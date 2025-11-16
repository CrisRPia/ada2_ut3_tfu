using Microsoft.EntityFrameworkCore;
using service.src.common.models;

namespace service.src.songs.services;

public class DatabaseSyncService(
    IServiceProvider serviceProvider,
    SongCacheService songCacheService
) : BackgroundService
{
    private readonly TimeSpan _syncInterval = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_syncInterval, stoppingToken);
            try {
                await Iteration(stoppingToken);
            } catch (Exception e) {
                Console.Error.WriteLine(e);
            }
        }
    }

    private async Task Iteration(CancellationToken stoppingToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // --- 1. Sync Local Diffs to Database ---
        var diffsToSync = songCacheService.GetAndResetDiffs();
        if (!diffsToSync.IsEmpty)
        {
            var updateTasks = new List<Task>();
            foreach (var diff in diffsToSync)
            {
                var songId = diff.Key;
                var playCountIncrease = diff.Value;

                updateTasks.Add(
                    dbContext.Database.ExecuteSqlAsync(
                        $"UPDATE \"Songs\" SET \"PlayCount\" = \"PlayCount\" + {playCountIncrease} WHERE \"Id\" = {songId}",
                        stoppingToken
                    )
                );
            }
            await Task.WhenAll(updateTasks);
            Console.WriteLine(
                $"[DatabaseSyncService] Synced play count diffs for {diffsToSync.Count} song(s)."
            );
        }

        // --- 2. Full Cache Refresh from Database ---

        var allSongsFromDb = await dbContext.Songs.ToListAsync(stoppingToken);

        var changedCount = songCacheService.FullCacheRefresh(allSongsFromDb);

        if (changedCount > 0)
        {
            Console.WriteLine(
                $"[DatabaseSyncService] Refreshed/added {changedCount} song(s) in the read cache."
            );
        }
    }
}
