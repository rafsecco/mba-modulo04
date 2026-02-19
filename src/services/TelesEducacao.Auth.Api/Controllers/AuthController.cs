using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Auth.Application.Services;
using TelesEducacao.Auth.Application.Dtos;
using TelesEducacao.Auth.Application.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace TelesEducacao.Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly IJwtService _jwtService;

    public AuthController(AuthService authService, IJwtService jwtService)
    {
        _authService = authService;
        _jwtService = jwtService;
    }

    [HttpPost("registrar")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] RegisterUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            return BadRequest(new
            {
                success = false,
                message = "Dados inválidos",
                errors = errors
            });
        }

        var result = await _authService.RegisterAsync(dto);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                success = false,
                message = result.Message,
                errors = result.Errors
            });
        }

        return Created("", new
        {
            success = true,
            message = result.Message,
            data = new { userId = result.Data }
        });
    }

    [HttpPost("acessar")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Acessar([FromBody] LoginUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .SelectMany(x => x.Value!.Errors)
                .Select(x => x.ErrorMessage)
                .ToList();

            return BadRequest(new
            {
                success = false,
                message = "Dados inválidos",
                errors = errors
            });
        }

        var loginResult = await _authService.LoginAsync(dto);

        if (!loginResult.IsSuccess)
        {
            return Unauthorized(new
            {
                success = false,
                message = loginResult.Message,
                errors = loginResult.Errors
            });
        }

        // Busca o usuário para gerar o JWT
        var userResult = await _authService.AuthenticateAsync(dto.Email, dto.Senha);
        if (!userResult.IsSuccess)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Erro ao gerar token de acesso"
            });
        }

        var user = await _authService.GetUserByIdAsync(loginResult.Data.ToString()!);
        if (user == null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Usuário não encontrado"
            });
        }

        var tokens = await _jwtService.GenerateTokensAsync(user);

        return Ok(new
        {
            success = true,
            message = "Login realizado com sucesso",
            data = tokens
        });
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return BadRequest(new
            {
                success = false,
                message = "Refresh token é obrigatório"
            });
        }

        var result = await _jwtService.RefreshTokenAsync(request.RefreshToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                success = false,
                message = result.Message,
                errors = result.Errors
            });
        }

        return Ok(new
        {
            success = true,
            message = result.Message,
            data = result.Data
        });
    }

    [HttpPost("revogar-token")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevogarToken([FromBody] RefreshTokenRequest request)
    {
        var result = await _jwtService.RevokeTokenAsync(request.RefreshToken, "Revogado pelo usuário");

        if (!result.IsSuccess)
        {
            return BadRequest(new
            {
                success = false,
                message = result.Message
            });
        }

        return Ok(new
        {
            success = true,
            message = result.Message
        });
    }

    [HttpPost("revogar-todos-tokens")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RevogarTodosTokens()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new
            {
                success = false,
                message = "Usuário não identificado"
            });
        }

        var result = await _jwtService.RevokeAllUserTokensAsync(userId, "Todos os tokens revogados pelo usuário");

        return Ok(new
        {
            success = true,
            message = result.Message
        });
    }

    [HttpPost("sair")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Sair([FromBody] RefreshTokenRequest request)
    {
        // Revoga o refresh token específico
        await _jwtService.RevokeTokenAsync(request.RefreshToken, "Logout do usuário");

        return Ok(new
        {
            success = true,
            message = "Logout realizado com sucesso"
        });
    }
}