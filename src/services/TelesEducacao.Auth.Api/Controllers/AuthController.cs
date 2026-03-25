using MediatR;
using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Auth.Application.Dtos;
using TelesEducacao.Auth.Application.Models;
using TelesEducacao.Auth.Application.Services;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;
using TelesEducacao.WebAPI.Core.Controllers;

namespace TelesEducacao.Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : MainController
{
    private readonly AuthService _authService;

    public AuthController(INotificationHandler<DomainNotification> notifications, IMediatorHandler mediatorHandler, AuthService authService)
        : base(mediatorHandler, notifications)
    {
        _authService = authService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Registrar([FromBody] RegisterUserDto dto, CancellationToken cancellationToken)
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

        var result = await _authService.RegistrarAsync(dto.Email, dto.Senha, dto.Role, cancellationToken);

        if (!result.HasValue)
        {
            return BadRequest(new
            {
                success = false,
            });
        }

        return Created("", new
        {
            result
        });
    }

    [HttpPost("acessar")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Acessar([FromBody] LoginUserDto dto, CancellationToken cancellationToken)
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

        var loginResult = await _authService.LoginAsync(dto.Email, dto.Senha, cancellationToken);

        return Ok(loginResult);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
    {
        var result = await _authService.ObterRefreshToken(Guid.Parse(refreshToken));

        return Ok(new
        {
            result
        });
    }
}