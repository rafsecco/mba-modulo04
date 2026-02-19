using System.ComponentModel.DataAnnotations;

namespace TelesEducacao.Auth.Domain.Entities
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; } = false;

        public string? RevokedReason { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? DeviceInfo { get; set; }

        public string? IpAddress { get; set; }

        public bool IsValid => !IsRevoked && DateTime.UtcNow < ExpiresAt;
    }
}