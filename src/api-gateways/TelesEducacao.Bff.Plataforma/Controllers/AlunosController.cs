using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Bff.Plataforma.Dtos;
using TelesEducacao.Bff.Plataforma.Services;

namespace TelesEducacao.Bff.Plataforma.Controllers;

[Authorize]
[Route("[controller]")]
[Authorize(Roles = "Aluno")]
public class AlunosController : MainController
{
    private readonly IAlunoService _alunoService;
    private readonly IConteudoService _conteudoService;

    public AlunosController(IAlunoService alunoService, IConteudoService conteudoService)
    {
        _alunoService = alunoService;
        _conteudoService = conteudoService;
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

    [HttpGet("{matriculaId}/Matricula")]
    [ProducesResponseType(typeof(MatriculaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult> ObterMatriculaPorId(Guid matriculaId,
    CancellationToken cancellationToken)
    {
        var matriculaDto = await _alunoService.ObterMatriculaPorId(matriculaId, cancellationToken);
        return Ok(matriculaDto);
    }

    [HttpGet("{matriculaId}/Matricula/AulasConcluidas")]
    [ProducesResponseType(typeof(IEnumerable<AulaConcluidaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> ObterAulasConcluidasPorMatriculaId(Guid matriculaId, CancellationToken cancellationToken)
    {
        var aulaConcluidaDtos = await _alunoService.ObterAulasConcluidasPorMatriculaId(matriculaId, cancellationToken);
        return Ok(aulaConcluidaDtos);
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

    [HttpPost("{matriculaId}/curso/{cursoId}/finalizar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SolicitarFinalizacaoCurso(
        Guid matriculaId,
        Guid cursoId,
        CancellationToken cancellationToken)
    {
        var aulasCurso = await _conteudoService.ObterAulasPorCurso(cursoId, cancellationToken);

        if (aulasCurso == null || !aulasCurso.Any())
        {
            return BadRequest(new { message = "Este curso não possui aulas cadastradas." });
        }

        var totalAulas = aulasCurso.Count();
        var result = await _alunoService.SolicitarFinalizacaoCurso(matriculaId, totalAulas, cancellationToken);

        if (!result)
        {
            return BadRequest(new { message = "Não foi possível concluir o curso. Verifique as pendências." });
        }

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("{id}/Matricula/{cursoId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AdicionarMatricula(Guid id, Guid cursoId, AdicionarMatriculaRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        if (id != requestDto.AlunoId || cursoId != requestDto.CursoId) return BadRequest();
        var curso = await _conteudoService.ObterPorId(cursoId, cancellationToken);

        if (curso == null) return BadRequest(new { message = "Curso não encontrado." });

        var matriculaCompletaDto = new AdicionarMatriculaDto
        {
            AlunoId = requestDto.AlunoId,
            CursoId = requestDto.CursoId,
            NomeCartao = requestDto.NomeCartao,
            NumeroCartao = requestDto.NumeroCartao,
            ExpiracaoCartao = requestDto.ExpiracaoCartao,
            CvvCartao = requestDto.CvvCartao,
            Valor = curso.Valor,
        };

        var result = await _alunoService.AdicionarMatricula(id, cursoId, matriculaCompletaDto, cancellationToken);
        if (result)
        {
            return StatusCode(StatusCodes.Status201Created);
        }

        return StatusCode(StatusCodes.Status400BadRequest);
    }
}