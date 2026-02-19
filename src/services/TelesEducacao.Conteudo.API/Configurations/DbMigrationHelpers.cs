using TelesEducacao.Conteudos.Data;
using TelesEducacao.Conteudos.Domain;

namespace TelesEducacao.Conteudo.API.Configurations;

public static class DbMigrationHelperExtension
{
	public static void UseDbMigrationHelper(this WebApplication app)
	{
		DbMigrationHelpers.EnsureSeedData(app).Wait();
	}
}

public static class DbMigrationHelpers
{
	/// <summary>
	///		Na pasta raiz do projeto execute os comandos abaixo para gerar as migrations:
	///		dotnet ef migrations add Initial_Conteudo --project ./src/services/TelesEducacao.Conteudo.Data --startup-project ./src/services/TelesEducacao.Conteudo.API --context ConteudosContext --configuration "Development"
	///		dotnet ef database update --project ./src/services/TelesEducacao.Conteudo.Data --startup-project ./src/services/TelesEducacao.Conteudo.API --context ConteudosContext --configuration "Development"
	///		dotnet ef migrations remove --project ./src/services/TelesEducacao.Conteudo.Data --startup-project ./src/services/TelesEducacao.Conteudo.API --context ConteudosContext --configuration "Development"
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

		var curso1 = new Curso("Curso 1", "Curso 1 descrição", true, 100.0m, new ConteudoProgramatico("CP 1", "CP 1 descrição"));
		var curso2 = new Curso("Curso 2", "Curso 2 descrição", true, 200.50m, new ConteudoProgramatico("CP 2", "CP 2 descrição"));
		var curso3 = new Curso("Curso 3", "Curso 3 descrição", true, 300.0m, new ConteudoProgramatico("CP 3", "CP 3 descrição"));
		await context.Cursos.AddRangeAsync(curso1, curso2, curso3);

		var aula1 = new Aula("Aula 1 titulo", "Aula 1 conteudo", curso1.Id);
		var aula2 = new Aula("Aula 2 titulo", "Aula 2 conteudo", curso2.Id);
		var aula3 = new Aula("Aula 3 titulo", "Aula 3 conteudo", curso3.Id);
		await context.Aulas.AddRangeAsync(aula1, aula2, aula3);

		await context.SaveChangesAsync();
	}
}
