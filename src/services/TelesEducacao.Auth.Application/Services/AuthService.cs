using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetDevPack.Security.JwtSigningCredentials.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TelesEducacao.Auth.Application.Extensions;
using TelesEducacao.Auth.Application.Models;
using TelesEducacao.Auth.Data;
using TelesEducacao.Auth.Data.Models;
using TelesEducacao.Core.Common.Constants;
using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;
using TelesEducacao.MessageBus;
using TelesEducacao.WebAPI.Core.Usuario;

namespace TelesEducacao.Auth.Application.Services;

public class AuthService
{
    private readonly AppTokenSettings _appTokenSettingsSettings;
    private readonly IAspNetUser _aspNetUser;
    private readonly AuthDbContext _context;

    private readonly IJsonWebKeySetService _jwksService;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IMessageBus _messageBus;

    public AuthService(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager,
        IOptions<AppTokenSettings> appTokenSettingsSettings, AuthDbContext context,
        IJsonWebKeySetService jwksService, IAspNetUser aspNetUser, RoleManager<IdentityRole> roleManager, IMessageBus messageBus)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _appTokenSettingsSettings = appTokenSettingsSettings.Value;
        _context = context;
        _jwksService = jwksService;
        _aspNetUser = aspNetUser;
        _roleManager = roleManager;
        _messageBus = messageBus;
    }

    public async Task<Guid?> RegistrarAsync(string email, string senha, string roleNome,
        CancellationToken cancellationToken)
    {
        var identityUser = new IdentityUser
        {
            Email = email,
            UserName = email
        };

        var result = await _userManager.CreateAsync(identityUser, senha);
        if (result.Succeeded)
        {
            await AdicionarRoleParaUsuario(identityUser, roleNome);
            if (AuthConstants.AlunoRole.Equals(roleNome, StringComparison.OrdinalIgnoreCase)) await RegistrarAlunoAsync(identityUser);

            return Guid.Parse(identityUser.Id);
        }

        return null;
    }

    public async Task<UsuarioRespostaLogin?> LoginAsync(string email, string senha, CancellationToken cancellationToken)
    {
        var result = await _signInManager.PasswordSignInAsync(email, senha, false, true);
        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is not null && !string.IsNullOrEmpty(user.Email)) return await GerarJwt(user.Email);
        }

        return null;
    }

    private async Task AdicionarRoleParaUsuario(IdentityUser user, string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role != null) await _userManager.AddToRoleAsync(user, roleName);
    }

    public async Task<UsuarioRespostaLogin> GerarJwt(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        var claims = await _userManager.GetClaimsAsync(user);

        var identityClaims = await ObterClaimsUsuario(claims, user);
        var encodedToken = CodificarToken(identityClaims);

        var refreshToken = await GerarRefreshToken(email);
        var resultToken = ObterRespostaToken(encodedToken, user, claims, refreshToken);

        return resultToken;
    }

    private async Task RegistrarAlunoAsync(IdentityUser user)
    {
        var usuarioRegistradoEvent = new UsuarioRegistradoIntegrationEvent(Guid.Parse(user.Id));

        try
        {
            var message = await _messageBus.RequestAsync<UsuarioRegistradoIntegrationEvent, ResponseMessage>(usuarioRegistradoEvent);
            if (!message.ValidationResult.IsValid)
            {
                await _userManager.DeleteAsync(user);
            }
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            throw;
        }
    }

    private async Task<ClaimsIdentity> ObterClaimsUsuario(ICollection<Claim> claims, IdentityUser user)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
        claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
        claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(),
            ClaimValueTypes.Integer64));
        foreach (var userRole in userRoles) claims.Add(new Claim("role", userRole));

        var identityClaims = new ClaimsIdentity();
        identityClaims.AddClaims(claims);

        return identityClaims;
    }

    private string CodificarToken(ClaimsIdentity identityClaims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var currentIssuer =
            $"{_aspNetUser.ObterHttpContext().Request.Scheme}://{_aspNetUser.ObterHttpContext().Request.Host}";
        var key = _jwksService.GetCurrent();
        var token = tokenHandler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = currentIssuer,
            Subject = identityClaims,
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = key
        });

        return tokenHandler.WriteToken(token);
    }

    private UsuarioRespostaLogin ObterRespostaToken(string encodedToken, IdentityUser user,
        IEnumerable<Claim> claims, RefreshToken refreshToken)
    {
        return new UsuarioRespostaLogin
        {
            AccessToken = encodedToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = TimeSpan.FromHours(1).TotalSeconds,
            UsuarioToken = new UsuarioToken
            {
                Id = user.Id,
                Email = user.Email,
                Claims = claims.Select(c => new UsuarioClaim { Type = c.Type, Value = c.Value })
            }
        };
    }

    private static long ToUnixEpochDate(DateTime date)
    {
        return (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
            .TotalSeconds);
    }

    private async Task<RefreshToken> GerarRefreshToken(string email)
    {
        var refreshToken = new RefreshToken
        {
            Username = email,
            ExpirationDate = DateTime.UtcNow.AddHours(_appTokenSettingsSettings.RefreshTokenExpiration)
        };

        _context.RefreshTokens.RemoveRange(_context.RefreshTokens.Where(u => u.Username == email));
        await _context.RefreshTokens.AddAsync(refreshToken);

        await _context.SaveChangesAsync();

        return refreshToken;
    }

    public async Task<RefreshToken> ObterRefreshToken(Guid refreshToken, CancellationToken cancellationToken)
    {
        var token = await _context.RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Token == refreshToken, cancellationToken);

        var expirationDate = token.ExpirationDate.ToLocalTime();

        return token != null && expirationDate > DateTime.Now
            ? token
            : null;
    }
}