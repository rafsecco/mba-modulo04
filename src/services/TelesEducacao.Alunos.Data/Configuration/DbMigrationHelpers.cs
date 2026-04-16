using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelesEducacao.Alunos.Domain;
using TelesEducacao.Core.Common.Constants;

namespace TelesEducacao.Alunos.Data.Configuration;

public static class DbMigrationHelperExtension
{
    public static void UseDbMigrationAlunosHelper(this IServiceProvider serviceProvider)
    {
        DbMigrationHelpers.EnsureSeedData(serviceProvider).Wait();
    }
}

public static class DbMigrationHelpers
{
    public static async Task EnsureSeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var alunosContext = scope.ServiceProvider.GetRequiredService<AlunosContext>();

        await alunosContext.Database.MigrateAsync();

        if (alunosContext.Alunos.Any()) return;

        await CriaAlunoAsync(alunosContext);
    }

    private static async Task CriaAlunoAsync(AlunosContext context)
    {
        var aluno = new Aluno(Guid.Parse(AuthConstants.SeedAlunoId));
        aluno.Ativar();

        context.Alunos.Add(aluno);

        await context.SaveChangesAsync();
    }
}