namespace TelesEducacao.Alunos.API.Models;

public class JwtSettings
{
    public string? Secret { get; set; }
    public int Duration { get; set; }
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
}