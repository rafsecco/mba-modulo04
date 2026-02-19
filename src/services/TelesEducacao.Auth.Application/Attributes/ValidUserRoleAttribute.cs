using System.ComponentModel.DataAnnotations;
using TelesEducacao.Auth.Application.Models;

namespace TelesEducacao.Auth.Application.Attributes
{
    public class ValidUserRoleAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
                return false;

            if (value is string roleString)
            {
                return Enum.TryParse<UserRole>(roleString, true, out _);
            }

            if (value is UserRole)
            {
                return Enum.IsDefined(typeof(UserRole), value);
            }

            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"O campo {name} deve ser 'Admin' ou 'Aluno'.";
        }
    }
}