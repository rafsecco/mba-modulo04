using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TelesEducacao.Auth.Application.Attributes
{
    public class StrongPasswordAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is not string password)
                return false;

            // Verifica se tem pelo menos 6 caracteres
            if (password.Length < 6)
                return false;

            // Verifica se tem pelo menos uma letra maiúscula
            if (!Regex.IsMatch(password, @"[A-Z]"))
                return false;

            // Verifica se tem pelo menos uma letra minúscula
            if (!Regex.IsMatch(password, @"[a-z]"))
                return false;

            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"O campo {name} deve ter pelo menos 6 caracteres, incluindo pelo menos uma letra maiúscula e uma minúscula.";
        }
    }
}