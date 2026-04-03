using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Bff.Plataforma.Dtos;
using TelesEducacao.Bff.Plataforma.Services;

namespace TelesEducacao.Bff.Plataforma.Controllers;

[Route("[controller]")]
public class AuthController : MainController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("registrar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Registrar([FromBody] RegistrarUsuarioDto registrarUsuarioDto,
        CancellationToken cancellationToken)
    {
        var resultado = await _authService.RegistrarAsync(registrarUsuarioDto, cancellationToken);
        return CreatedAtAction(nameof(Registrar), resultado);
    }

    [HttpPost("acessar")]
    [ProducesResponseType(typeof(UsuarioRespostaLogin), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto, CancellationToken cancellationToken)
    {
        var resultado = await _authService.LoginAsync(loginUserDto, cancellationToken);

        if (resultado == null) return Unauthorized(new { message = "E-mail ou senha incorretos." });

        return Ok(resultado);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(RefreshToken), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RefreshToken([FromBody] string refreshToken,
        CancellationToken cancellationToken)
    {
        var resultado = await _authService.RefreshTokenAsync(refreshToken, cancellationToken);
        if (resultado == null)
            return Unauthorized(new { message = "Sessão expirada. Por favor, faça login novamente." });
        return Ok(resultado);
    }
}