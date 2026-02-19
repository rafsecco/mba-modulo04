using System.ComponentModel.DataAnnotations;
using TelesEducacao.Auth.Application.Attributes;

namespace TelesEducacao.Auth.Application.Models
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória")]
        [StrongPassword]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role é obrigatória")]
        [ValidUserRole(ErrorMessage = "Role deve ser 'Admin' ou 'Aluno'")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nome é obrigatório")]
        public string Name { get; set; } = string.Empty;
    }
}