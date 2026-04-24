using TelesEducacao.Conteudos.Application.AutoMapper;
using TelesEducacao.Conteudos.Data;
using TelesEducacao.WebAPI.Core.Data;
using TelesEducacao.WebAPI.Core.Identidade;

namespace TelesEducacao.Conteudo.API.Configurations;

public static class ApiConfig
{
	public static IServiceCollection AddApiConfigurations(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
	{
		services.AddControllers();

		services.AddDatabase<ConteudosContext>(configuration, environment);

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

		services.AddJwtConfiguration(configuration);

		return services;
	}

	public static void UseApiCoreConfigurations(this WebApplication app)
	{
		if (!app.Environment.IsDevelopment())
		{
			app.UseHttpsRedirection();
		}

		app.UseRouting();

		app.UseCors("Total");

		app.UseAuthConfiguration();

		app.MapControllers();
	}

}
