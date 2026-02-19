using TelesEducacao.Conteudos.Application.Services;
using TelesEducacao.Conteudos.Data;
using TelesEducacao.Conteudos.Data.Repository;
using TelesEducacao.Conteudos.Domain;

namespace TelesEducacao.Conteudo.API.Configurations;

public static class DependencyInjectionConfig
{
	public static void RegisterServices(this IServiceCollection services)
	{
		services.AddScoped<ICargaHorariaService, CargaHorariaService>();
		services.AddScoped<ICursoRepository, CursoRepository>();
		services.AddScoped<ICursoAppService, CursoAppService>();
		services.AddScoped<ConteudosContext>();
	}
}
