using MediatR;
using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Auth.Application.Dtos;
using TelesEducacao.Auth.Application.Models;
using TelesEducacao.Auth.Application.Services;
using TelesEducacao.Auth.Data.Models;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;
using TelesEducacao.WebAPI.Core.Controllers;

namespace TelesEducacao.Auth.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : MainController
{
    private readonly AuthService _authService;

    public AuthController(INotificationHandler<DomainNotification> notifications, IMediatorHandler mediatorHandler, AuthService authService)
        : base(mediatorHandler, notifications)
    {
        _authService = authService;
    }

    [HttpPost("registrar")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _authService.RegistrarAsync(dto.Email, dto.Senha, dto.Role, cancellationToken);

        if (resultado == null || resultado == Guid.Empty)
        {
            return BadRequest(new
            {
                success = false,
                message = "Não foi possível realizar o registro. Verifique os dados e tente novamente."
            });
        }

        return CreatedAtAction(nameof(Registrar), new { id = resultado }, resultado);
    }

    [HttpPost("acessar")]
    [ProducesResponseType(typeof(UsuarioRespostaLogin), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Acessar([FromBody] LoginUserDto dto, CancellationToken cancellationToken)
    {
        var resultado = await _authService.LoginAsync(dto.Email, dto.Senha, cancellationToken);

        if (resultado == null)
        {
            return Unauthorized(new { message = "Usuário ou senha incorretos." });
        }

        return Ok(resultado);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(RefreshToken), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken, CancellationToken cancellationToken)
    {
        var resultado = await _authService.ObterRefreshToken(Guid.Parse(refreshToken), cancellationToken);

        if (resultado == null)
        {
            return Unauthorized(new { message = "Sessão expirada. Por favor, faça login novamente." });
        }

        return Ok(resultado);
    }
}