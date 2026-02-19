using TelesEducacao.Auth.Domain.Entities;

namespace TelesEducacao.Auth.Data.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task<List<RefreshToken>> GetUserTokensAsync(string userId);
        Task AddAsync(RefreshToken refreshToken);
        Task RevokeAsync(int tokenId, string reason);
        Task RevokeAllUserTokensAsync(string userId, string reason);
        Task CleanupExpiredTokensAsync();
    }
}