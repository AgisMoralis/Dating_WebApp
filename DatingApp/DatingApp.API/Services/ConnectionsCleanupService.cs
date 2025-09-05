using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Services;

/// <summary>
/// This service is responsible to get initiated once the application started and
/// should try to clean all the entries from the "Connections" table of the database.
/// This operation will try to execute maximum 10 times if it fails (once every 5 sec).
/// </summary>
public class ConnectionsCleanupService(IServiceProvider serviceProvider, ILogger<ConnectionsCleanupService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 10 attempts in total, one every 5 seconds to delete all entries from the "Connections" table
        var maxAttempts = 10;
        var delay = TimeSpan.FromSeconds(5);

        for (int attempt = 1; attempt <= maxAttempts && !stoppingToken.IsCancellationRequested; attempt++)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<Data.DataContext>();
            try
            {
                await context.Database.ExecuteSqlRawAsync("DELETE FROM [Connections]");
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $"Failed to clean up the \"Connections\" table on startup (Attempt {attempt}).");
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}
