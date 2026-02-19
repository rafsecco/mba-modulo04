using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelesEducacao.Alunos.Domain;

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
        //TODO: verificar depois como vai ser o seed de alunos, visto que tera uma api pra registro de alunos
        //var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        //var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await alunosContext.Database.MigrateAsync();
        //await EnsureSeedRoles(alunosContext, roleManager);
        //await EnsureUsers(alunosContext, userManager);
    }

    private static async Task CreateUserWithRoleAsync(AlunosContext context, UserManager<IdentityUser> userManager,
        string email, string password, string roleName)
    {
        var user = new IdentityUser
        {
            Email = email,
            UserName = email
        };
        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, roleName);
        }
        else
        {
            throw new Exception(
                $"Falha ao criar o usuário identity {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        var userId = Guid.Parse(user.Id);

        switch (roleName)
        {
            case "Admin": break;
            case "Aluno":
                var aluno = new Aluno(userId);
                await context.Alunos.AddAsync(aluno);
                break;

            default:
                throw new Exception("Tipo de usuário não encontrado!");
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureUsers(AlunosContext context, UserManager<IdentityUser> userManager)
    {
        //if (context.Users.Any())
        //    return;

        await CreateUserWithRoleAsync(context, userManager, "admin@mail.com", "Dev@123", "Admin");
        await CreateUserWithRoleAsync(context, userManager, "aluno1@mail.com", "Dev@123", "Aluno");
        await CreateUserWithRoleAsync(context, userManager, "aluno2@mail.com", "Dev@123", "Aluno");
    }

    private static async Task EnsureSeedRoles(AlunosContext context, RoleManager<IdentityRole> roleManager)
    {
        await CreateRoleAsync(roleManager, "Admin");
        await CreateRoleAsync(roleManager, "Aluno");
        await context.SaveChangesAsync();
    }

    private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
            return;

        var role = new IdentityRole(roleName);
        await roleManager.CreateAsync(role);
    }
}