namespace TelesEducacao.Conteudo.API.Configurations;

public static class ApiConfig
{
	public static IServiceCollection AddApiConfigurations(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddControllers();

		services.AddCors(options =>
		{
			options.AddPolicy("Total",
				builder =>
					builder
						.AllowAnyOrigin()
						.AllowAnyMethod()
						.AllowAnyHeader());
		});

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
