using System.ComponentModel.DataAnnotations;

namespace TelesEducacao.Bff.Plataforma.Dtos;

public class RegistrarUsuarioDto
{
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [EmailAddress(ErrorMessage = "O campo {0} está em formato inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [StringLength(100, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 6)]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    public string Role { get; set; } = string.Empty;
}

public class LoginUserDto
{
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [EmailAddress(ErrorMessage = "O campo {0} está em formato inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [StringLength(100, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 6)]
    public string Senha { get; set; } = string.Empty;
}

public class UsuarioClaim
{
    public string Value { get; set; }
    public string Type { get; set; }
}

public class UsuarioToken
{
    public string Id { get; set; }
    public string Email { get; set; }
    public IEnumerable<UsuarioClaim> Claims { get; set; }
}

public class UsuarioRespostaLogin
{
    public string AccessToken { get; set; }
    public Guid RefreshToken { get; set; }
    public double ExpiresIn { get; set; }
    public UsuarioToken UsuarioToken { get; set; }
}

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public Guid Token { get; set; }
    public DateTime ExpirationDate { get; set; }
}