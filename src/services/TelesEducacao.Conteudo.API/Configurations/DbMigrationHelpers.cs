using TelesEducacao.Conteudos.Data;
using TelesEducacao.Conteudos.Domain;

namespace TelesEducacao.Conteudo.API.Configurations;

public static class DbMigrationHelpers
{
	/// <summary>
	///     Generate migrations before running this method, you can use command bellow:
	///     Nuget package manager: Add-Migration DbInit -context ConteudosContext
	///     Dotnet CLI: dotnet ef migrations add DbInit -c ConteudosContext
	/// </summary>
	public static async Task EnsureSeedData(WebApplication serviceScope)
	{
		var services = serviceScope.Services.CreateScope().ServiceProvider;
		await EnsureSeedData(services);
	}

	public static async Task EnsureSeedData(IServiceProvider serviceProvider)
	{
		using var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
		var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

		var context = scope.ServiceProvider.GetRequiredService<ConteudosContext>();

		if (env.IsDevelopment() || env.IsEnvironment("Docker"))
		{
			await context.Database.EnsureCreatedAsync();
			await EnsureSeed(context);
		}
	}

	private static async Task EnsureSeed(ConteudosContext context)
	{
		if (context.Cursos.Any())
			return;

		if (context.Aulas.Any())
			return;

		var curso1 = new Curso("Curso 1", "Curso 1 descrição", true, 100m, new ConteudoProgramatico("CP 1", "CP 1 descrição"));
		var curso2 = new Curso("Curso 2", "Curso 2 descrição", true, 200.50m, new ConteudoProgramatico("CP 2", "CP 2 descrição"));
		var curso3 = new Curso("Curso 3", "Curso 3 descrição", true, 300m, new ConteudoProgramatico("CP 3", "CP 3 descrição"));
		await context.Cursos.AddRangeAsync(curso1, curso2, curso3);

		var aula1 = new Aula("Aula 1 titulo", "Aula 1 conteudo", curso1.Id);
		var aula2 = new Aula("Aula 2 titulo", "Aula 2 conteudo", curso2.Id);
		var aula3 = new Aula("Aula 3 titulo", "Aula 3 conteudo", curso3.Id);
		await context.Aulas.AddRangeAsync(aula1, aula2, aula3);

		await context.SaveChangesAsync();
	}
}
