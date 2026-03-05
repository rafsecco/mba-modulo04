using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Bff.Plataforma.Dtos;
using TelesEducacao.Bff.Plataforma.Services;

namespace TelesEducacao.Bff.Plataforma.Controllers;

[Authorize]
[Route("[controller]")]
public class AlunosController : MainController
{
    private readonly IAlunoService _alunoService;

    public AlunosController(IAlunoService alunoService)
    {
        _alunoService = alunoService;
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AlunoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<AlunoDto>> ObterAlunoPorId(Guid id,
        CancellationToken cancellationToken)
    {
        var aluno = await _alunoService.ObterPorId(id, cancellationToken);

        if (aluno == null)
            return NotFound(new { message = "Aluno não encontrado." });

        return Ok(aluno);
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AlunoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult> ObterTodos(
        CancellationToken cancellationToken)
    {
        var alunos = await _alunoService.ObterTodos(cancellationToken);
        return Ok(alunos);
    }

    [HttpGet("{id}/Matriculas")]
    [ProducesResponseType(typeof(IEnumerable<MatriculaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult> ObterMatriculasPorAlunoId(Guid id,
        CancellationToken cancellationToken)
    {
        var matriculaDtos = await _alunoService.ObterMatriculasPorAlunoId(id, cancellationToken);
        return Ok(matriculaDtos);
    }

    [HttpPost("{id}/Matricula/{aulaId}/AulasConcluidas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ConcluirAula(Guid id, Guid aulaId,
        CancellationToken cancellationToken)
    {
        var result = await _alunoService.ConcluirAula(id, aulaId, cancellationToken);
        if (result)
        {
            return StatusCode(StatusCodes.Status201Created);
        }

        return StatusCode(StatusCodes.Status400BadRequest);
    }

    [HttpPost("{matriculaId}/Matricula/Certificados")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SolicitarFinalizacaoCurso(Guid matriculaId,
        CancellationToken cancellationToken)
    {
        //obter a matricula do aluno
        //var matricula = await _alunoQueries.ObterMatriculaPorId(id);

        //obter as aulas do curso
        //var aulasCurso = await _cursoAppService.ObterAulas(matricula.CursoId);

        //if (aulasCurso == null || !aulasCurso.Any())
        //    return BadRequest(new { message = "Este curso não possui aulas cadastradas." });

        //obter as aulas concluidas do aluno

        //var aulasConcluidas = await _alunoQueries.ObterAulasConcluidasPorMatriculaId(matricula.Id);

        //var totalAulasCurso = aulasCurso.Count();
        //var totalConcluidas = aulasConcluidas.Count();

        //if (totalConcluidas < totalAulasCurso)
        //    return BadRequest(new
        //    {
        //        message =
        //            $"Não é possível concluir o curso. Você concluiu {totalConcluidas} de {totalAulasCurso} aulas."
        //    });

        var result = await _alunoService.SolicitarFinalizacaoCurso(matriculaId, cancellationToken);

        if (result)
        {
            return StatusCode(StatusCodes.Status201Created);
        }

        return StatusCode(StatusCodes.Status400BadRequest);
    }

    [HttpPost("{id}/Matricula/{cursoId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AdicionarMatricula(Guid id, Guid cursoId, AdicionarMatriculaRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        if (id != requestDto.AlunoId || cursoId != requestDto.CursoId) return BadRequest();

        //buscar o curso para obter o valor
        //var curso = await _cursoAppService.ObterPorId(cursoId);

        //if (curso == null) return BadRequest(new { message = "Curso não encontrado." });

        var matriculaCompletaDto = new AdicionarMatriculaDto
        {
            AlunoId = requestDto.AlunoId,
            CursoId = requestDto.CursoId,
            NomeCartao = requestDto.NomeCartao,
            NumeroCartao = requestDto.NumeroCartao,
            ExpiracaoCartao = requestDto.ExpiracaoCartao,
            CvvCartao = requestDto.CvvCartao,
            Valor = 100//curso.Valor;
        };

        var result = await _alunoService.AdicionarMatricula(id, cursoId, matriculaCompletaDto, cancellationToken);
        if (result)
        {
            return StatusCode(StatusCodes.Status201Created);
        }

        return StatusCode(StatusCodes.Status400BadRequest);
    }
}