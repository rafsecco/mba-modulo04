using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetDevPack.Security.JwtSigningCredentials;
using NetDevPack.Security.JwtSigningCredentials.Store.EntityFrameworkCore;
using System.Reflection;
using TelesEducacao.Auth.Data.Models;
using TelesEducacao.Core.Messages;

namespace TelesEducacao.Auth.Data;

//ISecurityKeyContext: implementar essa interface, garante que este contexto de banco de dados possui as propriedades necessárias para gerenciar chaves de segurança.
public class AuthDbContext : IdentityDbContext<IdentityUser>, ISecurityKeyContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<SecurityKeyWithPrivate> SecurityKeys { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //HACK: pra não setar string como varchar(max)
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(
            e => e.GetProperties().Where(p => p.ClrType == typeof(string))))
        {
            property.SetMaxLength(100);
        }

        modelBuilder.Ignore<Event>();

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}