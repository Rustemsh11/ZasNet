using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ZasNet.Infrastruture.Persistence;

public static class MigrationManager
{
    private static int _numberOfRetries;

    public static IHost MigrateDatabase(this IHost host)
    {
        using (var scope = host.Services.CreateScope())
        {
            using var appContext =
          scope.ServiceProvider.GetRequiredService<ZasNetDbContext>();
            try
            {
                appContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                if (_numberOfRetries < 6)
                {
                    Thread.Sleep(10000);

                    _numberOfRetries++;
                    Console.WriteLine($"The server was not found or was not accessible. Retrying... #{_numberOfRetries}");
                    Console.WriteLine($"Error: {ex.Message}");
                    return MigrateDatabase(host);
                }
                
                // Если все 6 попыток не удались - тогда выбрасываем
                Console.WriteLine($"Failed to migrate database after {_numberOfRetries} attempts");
                throw;
            }
            return host;
        }
    }
}
