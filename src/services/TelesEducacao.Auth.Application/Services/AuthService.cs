using Microsoft.AspNetCore.Identity;
using TelesEducacao.Auth.Application.Dtos;
using TelesEducacao.Auth.Application.Models;

namespace TelesEducacao.Auth.Application.Services
{
    public class AuthService
    {        
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService(            
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {            
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<AuthResultDto<UserDto>> AuthenticateAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                return AuthResultDto<UserDto>.Failure("Email é obrigatório");

            if (string.IsNullOrWhiteSpace(password))
                return AuthResultDto<UserDto>.Failure("Senha é obrigatória");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return AuthResultDto<UserDto>.Failure("Email não encontrado");

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    return AuthResultDto<UserDto>.Failure("Conta bloqueada temporariamente");
                
                if (result.IsNotAllowed)
                    return AuthResultDto<UserDto>.Failure("Login não permitido. Confirme seu email");
                
                return AuthResultDto<UserDto>.Failure("Email ou senha incorretos");
            }

            var userDto = new UserDto { Id = user.Id, Email = user.Email! };
            return AuthResultDto<UserDto>.Success(userDto, "Login realizado com sucesso");
        }

        public async Task<AuthResultDto<Guid>> RegisterAsync(RegisterUserDto registerUserDto)
        {
            if (registerUserDto == null)
                return AuthResultDto<Guid>.Failure("Dados de registro são obrigatórios");

            // Verifica se email já existe
            var existingUser = await _userManager.FindByEmailAsync(registerUserDto.Email);
            if (existingUser != null)
                return AuthResultDto<Guid>.Failure("Email já está em uso");

            // Verifica se role existe
            var roleExists = await _roleManager.RoleExistsAsync(registerUserDto.Role);
            if (!roleExists)
                return AuthResultDto<Guid>.Failure($"Perfil '{registerUserDto.Role}' não existe");

            var identityUser = new IdentityUser
            {
                Email = registerUserDto.Email,
                UserName = registerUserDto.Email,
                EmailConfirmed = true // Para desenvolvimento
            };

            var result = await _userManager.CreateAsync(identityUser, registerUserDto.Senha);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return AuthResultDto<Guid>.Failure($"Erro ao criar usuário: {errors}");
            }

            var addRoleResult = await _userManager.AddToRoleAsync(identityUser, registerUserDto.Role);
            if (!addRoleResult.Succeeded)
            {
                // Se falhou ao adicionar role, remove o usuário criado
                await _userManager.DeleteAsync(identityUser);
                var errors = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                return AuthResultDto<Guid>.Failure($"Erro ao atribuir perfil: {errors}");
            }

            return AuthResultDto<Guid>.Success(Guid.Parse(identityUser.Id), "Usuário criado com sucesso");
        }

        public async Task<AuthResultDto<Guid>> LoginAsync(LoginUserDto loginUserDto)
        {
            if (loginUserDto == null)
                return AuthResultDto<Guid>.Failure("Dados de login são obrigatórios");

            var result = await _signInManager.PasswordSignInAsync(
                loginUserDto.Email, 
                loginUserDto.Senha, 
                isPersistent: false, 
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    return AuthResultDto<Guid>.Failure("Conta bloqueada devido a muitas tentativas de login inválidas");
                
                if (result.IsNotAllowed)
                    return AuthResultDto<Guid>.Failure("Login não permitido. Confirme seu email");
                
                if (result.RequiresTwoFactor)
                    return AuthResultDto<Guid>.Failure("Autenticação de dois fatores necessária");

                return AuthResultDto<Guid>.Failure("Email ou senha incorretos");
            }

            var user = await _userManager.FindByEmailAsync(loginUserDto.Email);
            if (user == null)
                return AuthResultDto<Guid>.Failure("Usuário não encontrado");

            return AuthResultDto<Guid>.Success(Guid.Parse(user.Id), "Login realizado com sucesso");
        }

        public async Task<IdentityUser?> GetUserByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IdentityUser?> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<AuthResultDto<bool>> AddRoleToUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return AuthResultDto<bool>.Failure("Usuário não encontrado");

            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
                return AuthResultDto<bool>.Failure($"Perfil '{roleName}' não existe");

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (isInRole)
                return AuthResultDto<bool>.Failure($"Usuário já possui o perfil '{roleName}'");

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return AuthResultDto<bool>.Failure($"Erro ao atribuir perfil: {errors}");
            }

            return AuthResultDto<bool>.Success(true, "Perfil atribuído com sucesso");
        }

        public async Task<AuthResultDto<List<string>>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return AuthResultDto<List<string>>.Failure("Usuário não encontrado");

            var roles = await _userManager.GetRolesAsync(user);
            return AuthResultDto<List<string>>.Success(roles.ToList(), "Perfis obtidos com sucesso");
        }

        public async Task<AuthResultDto<bool>> RemoveRoleFromUserAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return AuthResultDto<bool>.Failure("Usuário não encontrado");

            var isInRole = await _userManager.IsInRoleAsync(user, roleName);
            if (!isInRole)
                return AuthResultDto<bool>.Failure($"Usuário não possui o perfil '{roleName}'");

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return AuthResultDto<bool>.Failure($"Erro ao remover perfil: {errors}");
            }

            return AuthResultDto<bool>.Success(true, "Perfil removido com sucesso");
        }

        public async Task<AuthResultDto<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return AuthResultDto<bool>.Failure("Usuário não encontrado");

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return AuthResultDto<bool>.Failure($"Erro ao alterar senha: {errors}");
            }

            return AuthResultDto<bool>.Success(true, "Senha alterada com sucesso");
        }

        public async Task<AuthResultDto<bool>> ResetPasswordAsync(string userId, string token, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return AuthResultDto<bool>.Failure("Usuário não encontrado");

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return AuthResultDto<bool>.Failure($"Erro ao redefinir senha: {errors}");
            }

            return AuthResultDto<bool>.Success(true, "Senha redefinida com sucesso");
        }

        public async Task<AuthResultDto<string>> GeneratePasswordResetTokenAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return AuthResultDto<string>.Failure("Usuário não encontrado");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return AuthResultDto<string>.Success(token, "Token de redefinição gerado com sucesso");
        }

        public async Task<AuthResultDto<bool>> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return AuthResultDto<bool>.Failure("Usuário não encontrado");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return AuthResultDto<bool>.Failure($"Erro ao confirmar email: {errors}");
            }

            return AuthResultDto<bool>.Success(true, "Email confirmado com sucesso");
        }

        public async Task<AuthResultDto<UserDto>> GetUserInfoAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return AuthResultDto<UserDto>.Failure("Usuário não encontrado");

            var userDto = new UserDto 
            { 
                Id = user.Id, 
                Email = user.Email! 
            };

            return AuthResultDto<UserDto>.Success(userDto, "Informações do usuário obtidas com sucesso");
        }
    }
}