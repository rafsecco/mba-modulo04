using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TelesEducacao.WebAPI.Core.Database;

public static class DatabaseExtensions
{
    public static IServiceCollection AddDatabase<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
        where TContext : DbContext
    {
        services.AddDbContext<TContext>(options =>
        {
            if (environment.IsDevelopment())
            {
                var connection = configuration.GetConnectionString("Sqlite");

                if (string.IsNullOrEmpty(connection))
                    throw new InvalidOperationException("Connection SQLite não configurada.");

                //Ignora o erro de mudanças pendentes no SQLite
                options.UseSqlite(connection).ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            }
            else
            {
                // Para Docker, Staging e Production, tenta DefaultConnection primeiro, depois SqlServer
                string? connection = null;

                if (environment.EnvironmentName == "Docker")
                {
                    connection = configuration.GetConnectionString("DefaultConnection");
                }

                // Fallback para SqlServer se DefaultConnection não existir ou se não for Docker
                if (string.IsNullOrEmpty(connection))
                {
                    connection = configuration.GetConnectionString("SqlServer");
                }

                if (string.IsNullOrEmpty(connection))
                    throw new InvalidOperationException($"Connection Database não configurada para o ambiente {environment.EnvironmentName}.");

                options.UseSqlServer(connection);
            }
        });

        return services;
    }
}
