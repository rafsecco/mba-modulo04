using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Bff.Plataforma.Dtos;
using TelesEducacao.Bff.Plataforma.Services;

namespace TelesEducacao.Bff.Plataforma.Controllers;

[Authorize]
public class AlunosController : MainController
{
    private readonly IAlunoService _alunoService;

    public AlunosController(IAlunoService alunoService)
    {
        _alunoService = alunoService;
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AlunoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)] // Opcional: erro de comunicação
    public async Task<ActionResult<AlunoDto>> ObterAlunoPorId(Guid id,
        CancellationToken cancellationToken)
    {
        var aluno = await _alunoService.ObterPorId(id, cancellationToken);

        if (aluno == null)
            return NotFound(new { message = "Aluno não encontrado no serviço de origem." });

        return Ok(aluno);
    }
}