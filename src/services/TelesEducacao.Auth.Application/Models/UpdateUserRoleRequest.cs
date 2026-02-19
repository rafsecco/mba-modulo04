using System.ComponentModel.DataAnnotations;
using TelesEducacao.Auth.Application.Attributes;

namespace TelesEducacao.Auth.Application.Models
{
    public class UpdateUserRoleRequest
    {
        [Required(ErrorMessage = "UserId é obrigatório")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role é obrigatória")]
        [ValidUserRole(ErrorMessage = "Role deve ser 'Admin' ou 'Aluno'")]
        public string Role { get; set; } = string.Empty;
    }
}