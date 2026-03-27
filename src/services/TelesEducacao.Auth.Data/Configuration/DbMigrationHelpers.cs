using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelesEducacao.Core.Common.Constants;

namespace TelesEducacao.Auth.Data.Configuration;

public static class DbMigrationHelperExtension
{
    public static void UseDbMigrationAuthHelper(this IServiceProvider serviceProvider)
    {
        DbMigrationHelpers.EnsureSeedData(serviceProvider).Wait();
    }
}

public static class DbMigrationHelpers
{
    public static async Task EnsureSeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var authContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await authContext.Database.MigrateAsync();
        await EnsureSeedRoles(roleManager);
        await EnsureUsers(authContext, userManager);
    }

    private static async Task EnsureUsers(AuthDbContext context, UserManager<IdentityUser> userManager)
    {
        if (context.Users.Any())
            return;

        await CreateUserWithRoleAsync(userManager, "admin@mail.com", AuthConstants.SeedSenhaPadrao, AuthConstants.AdminRole, Guid.NewGuid().ToString());
        await CreateUserWithRoleAsync(userManager, "aluno@mail.com", AuthConstants.SeedSenhaPadrao, AuthConstants.AlunoRole, AuthConstants.SeedAlunoId);
    }

    private static async Task EnsureSeedRoles(RoleManager<IdentityRole> roleManager)
    {
        await CreateRoleAsync(roleManager, "Admin");
        await CreateRoleAsync(roleManager, "Aluno");
    }

    private static async Task CreateRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
            return;

        var role = new IdentityRole(roleName);
        await roleManager.CreateAsync(role);
    }

    private static async Task CreateUserWithRoleAsync(UserManager<IdentityUser> userManager,
        string email, string password, string roleName, string userId)
    {
        var user = new IdentityUser
        {
            Email = email,
            UserName = email,
            Id = userId,
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
    }
}