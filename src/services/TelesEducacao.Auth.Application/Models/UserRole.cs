using System.ComponentModel.DataAnnotations;

namespace TelesEducacao.Auth.Application.Models
{
    public enum UserRole
    {
        [Display(Name = "Admin")]
        Admin,

        [Display(Name = "Aluno")]
        Aluno
    }
}