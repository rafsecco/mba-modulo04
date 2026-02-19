namespace TelesEducacao.Auth.Application.Dtos.Responses
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public UserDto? User { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}