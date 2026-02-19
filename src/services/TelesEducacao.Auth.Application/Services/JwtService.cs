using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TelesEducacao.Auth.Application.Models;
using TelesEducacao.Auth.Data.Repositories;
using TelesEducacao.Auth.Domain.Entities;

namespace TelesEducacao.Auth.Application.Services
{
    public interface IJwtService
    {
        Task<TokenResponse> GenerateTokensAsync(IdentityUser user);
        Task<AuthResultDto<TokenResponse>> RefreshTokenAsync(string refreshToken);
        Task<AuthResultDto<bool>> RevokeTokenAsync(string refreshToken, string reason);
        Task<AuthResultDto<bool>> RevokeAllUserTokensAsync(string userId, string reason);
        ClaimsPrincipal? ValidateToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public JwtService(
            IOptions<JwtSettings> jwtSettings,
            UserManager<IdentityUser> userManager,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<TokenResponse> GenerateTokensAsync(IdentityUser user)
        {
            var accessToken = await GenerateAccessTokenAsync(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);

            return new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                TokenType = "Bearer"
            };
        }

        public async Task<AuthResultDto<TokenResponse>> RefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (storedToken == null)
                return AuthResultDto<TokenResponse>.Failure("Refresh token inválido");

            if (!storedToken.IsValid)
                return AuthResultDto<TokenResponse>.Failure("Refresh token expirado ou revogado");

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            if (user == null)
                return AuthResultDto<TokenResponse>.Failure("Usuário não encontrado");

            // Revoga o token atual
            await _refreshTokenRepository.RevokeAsync(storedToken.Id, "Token usado para refresh");

            // Gera novos tokens
            var newTokens = await GenerateTokensAsync(user);

            return AuthResultDto<TokenResponse>.Success(newTokens, "Tokens renovados com sucesso");
        }

        public async Task<AuthResultDto<bool>> RevokeTokenAsync(string refreshToken, string reason)
        {
            var storedToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);

            if (storedToken == null)
                return AuthResultDto<bool>.Failure("Token não encontrado");

            await _refreshTokenRepository.RevokeAsync(storedToken.Id, reason);

            return AuthResultDto<bool>.Success(true, "Token revogado com sucesso");
        }

        public async Task<AuthResultDto<bool>> RevokeAllUserTokensAsync(string userId, string reason)
        {
            await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, reason);
            return AuthResultDto<bool>.Success(true, "Todos os tokens do usuário foram revogados");
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtSettings.SecretKey);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        private async Task<string> GenerateAccessTokenAsync(IdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.Name, user.UserName!),
                new("jti", Guid.NewGuid().ToString())
            };

            // Adiciona roles como claims
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}