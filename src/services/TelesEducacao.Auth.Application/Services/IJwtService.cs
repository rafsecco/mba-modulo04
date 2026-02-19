using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TelesEducacao.Auth.Application.Models;

namespace TelesEducacao.Auth.Application.Services
{
    public interface IJwtService
    {
        Task<TokenResponse> GenerateTokenAsync(IdentityUser user);        
        Task<AuthResultDto<TokenResponse>> RefreshTokenAsync(string refreshToken);
        ClaimsPrincipal? ValidateToken(string token);
    }
}