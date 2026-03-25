using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetDevPack.Security.JwtSigningCredentials;
using NetDevPack.Security.JwtSigningCredentials.Store.EntityFrameworkCore;
using TelesEducacao.Auth.Data.Models;

namespace TelesEducacao.Auth.Data;

//ISecurityKeyContext: implementar essa interface, garante que este contexto de banco de dados possui as propriedades necessárias para gerenciar chaves de segurança.
public class AuthDbContext : IdentityDbContext<IdentityUser>, ISecurityKeyContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<SecurityKeyWithPrivate> SecurityKeys { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
}