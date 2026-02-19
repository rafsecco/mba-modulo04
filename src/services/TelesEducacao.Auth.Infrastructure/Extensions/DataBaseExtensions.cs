using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelesEduaccao.Auth.Infrastructure.Data;

namespace TelesEducacao.Auth.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task<IServiceProvider> UseDatabaseMigrationAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<AuthDbContext>>();

        try
        {
            logger?.LogInformation("Verificando conexão com o banco de dados...");

            if (await IsDatabaseActiveAsync(context, logger))
            {
                logger?.LogInformation("Executando migrations...");
                await context.Database.MigrateAsync();
                logger?.LogInformation("Migrations executadas com sucesso.");
            }
            else
            {
                throw new InvalidOperationException("Não foi possível conectar ao banco de dados.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Erro durante a execução das migrations: {Message}", ex.Message);
            throw;
        }

        return serviceProvider;
    }

    private static async Task<bool> IsDatabaseActiveAsync(AuthDbContext context, ILogger? logger = null)
    {
        try
        {
            var canConnect = await context.Database.CanConnectAsync();

            if (canConnect)
            {
                logger?.LogInformation("Conexão com banco de dados estabelecida com sucesso.");
                return true;
            }

            logger?.LogError("Não foi possível estabelecer conexão com o banco de dados.");
            return false;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Erro ao verificar conexão: {Message}", ex.Message);
            return false;
        }
    }
}