using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
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
				var conn = configuration.GetConnectionString("Sqlite");

				if (string.IsNullOrEmpty(conn))
					throw new InvalidOperationException("Connection SQLite não configurada.");

				options.UseSqlite(conn);
			}
			else
			{
				var conn = configuration.GetConnectionString("SqlServer");

				if (string.IsNullOrEmpty(conn))
					throw new InvalidOperationException("Connection SQL Server não configurada.");

				options.UseSqlServer(conn);
			}
		});

		return services;
	}
}

