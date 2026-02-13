using Microsoft.OpenApi.Models;

namespace TelesEducacao.Conteudo.API.Configurations;

public static class SwaggerConfigs
{
	public static IServiceCollection AddSwaggerConfigureServices(this IServiceCollection services)
	{
		services.AddEndpointsApiExplorer();

		services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "TelesEducação API",
				Version = "v1",
				Description = "Documentação da API com autenticação JWT"
			});

			c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
			{
				Description = "Insira o Token JWT dessa forma: Bearer {seu token}",
				Name = "Authorization",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.Http,
				Scheme = "bearer",
				BearerFormat = "JWT"
			});

			c.AddSecurityRequirement(new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Scheme = "bearer",
						Name = "Authorization",
						In = ParameterLocation.Header,
						Type = SecuritySchemeType.Http
					},
					Array.Empty<string>()
				}
			});
		});

		return services;
	}

	public static WebApplication UseSwaggerConfiguration(this WebApplication app)
	{
		if (app.Environment.IsDevelopment())
		{
			app.UseSwagger();

			app.UseSwaggerUI(c =>
			{
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "TelesEducacao Conteúdo API v1");
				c.RoutePrefix = string.Empty;
			});
		}

		return app;
	}
}
