using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace TelesEducacao.Pagamentos.Data.Configuration;

public static class DbMigrationHelperExtension
{
    public static void UseDbMigrationPagamentosHelper(this IServiceProvider serviceProvider)
    {
        DbMigrationHelpers.EnsureSeedData(serviceProvider).Wait();
    }
}

public static class DbMigrationHelpers
{
    public static async Task EnsureSeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var conteudosContext = scope.ServiceProvider.GetRequiredService<PagamentosContext>();

        await conteudosContext.Database.MigrateAsync();
    }
}