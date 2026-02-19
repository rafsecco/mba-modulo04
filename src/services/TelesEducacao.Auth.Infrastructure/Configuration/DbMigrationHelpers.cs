using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace TelesEducacao.Auth.Infrastructure.Data;

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

        var authDbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await authDbContext.Database.MigrateAsync();
        await EnsureSeedRoles(authDbContext, roleManager);
        await EnsureUsers(authDbContext, userManager);
    }
       

    private static async Task CreateUserWithRoleAsync(AuthDbContext context, UserManager<IdentityUser> userManager,
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
    }

    private static async Task EnsureUsers(AuthDbContext context, UserManager<IdentityUser> userManager)
    {
        if (context.Users.Any())
            return;

        await CreateUserWithRoleAsync(context, userManager, "admin@mail.com", "Dev@123", "Admin");
        
    }

    private static async Task EnsureSeedRoles(AuthDbContext context, RoleManager<IdentityRole> roleManager)
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