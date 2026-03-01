using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TelesEducacao.Conteudos.Application.AutoMapper;
using TelesEducacao.Conteudos.Data;

namespace TelesEducacao.Conteudo.API.Configurations;

public static class ApiConfig
{
	public static IServiceCollection AddApiConfigurations(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddControllers();

		services.AddDbContext<ConteudosContext>(options =>
			options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

		services.AddCors(options =>
		{
			options.AddPolicy("Total",
				builder =>
					builder
						.AllowAnyOrigin()
						.AllowAnyMethod()
						.AllowAnyHeader());
		});

		services.AddAutoMapper(cfg => { },typeof(DtoToDomainMappingProfile),
			typeof(DomainToDtoMappingProfile));

		services.AddMediatR(cfg =>
		{
			//cfg.RegisterServicesFromAssembly(typeof(CriarCursoCommandHandler).Assembly);
			cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
		});

		services.AddMessageBusConfiguration(configuration);

		return services;
	}

	public static void UseApiCoreConfigurations(this WebApplication app)
	{
		app.UseHttpsRedirection();

		app.UseRouting();

		app.UseCors("Total");

		app.UseAuthorization();

		app.MapControllers();
	}

}
