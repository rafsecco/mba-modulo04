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
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AlunoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<AlunoDto>> ObterAlunoPorId(Guid id,
        CancellationToken cancellationToken)
    {
        var aluno = await _alunoService.ObterPorIdAsync(id, cancellationToken);

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
        var alunos = await _alunoService.ObterTodosAsync(cancellationToken);
        return Ok(alunos);
    }

    [HttpGet("{id:guid}/matriculas")]
    [ProducesResponseType(typeof(IEnumerable<MatriculaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult> ObterMatriculasPorAlunoId(Guid id,
        CancellationToken cancellationToken)
    {
        var matriculaDtos = await _alunoService.ObterMatriculasPorAlunoIdAsync(id, cancellationToken);
        return Ok(matriculaDtos);
    }

    [HttpGet("matriculas/{matriculaId:guid}")]
    [ProducesResponseType(typeof(MatriculaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult> ObterMatriculaPorId(Guid matriculaId,
    CancellationToken cancellationToken)
    {
        var matriculaDto = await _alunoService.ObterMatriculaPorIdAsync(matriculaId, cancellationToken);
        return Ok(matriculaDto);
    }

    [HttpGet("matriculas/{matriculaId:guid}/aulas-concluidas")]
    [ProducesResponseType(typeof(IEnumerable<AulaConcluidaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> ObterAulasConcluidasPorMatriculaId(Guid matriculaId, CancellationToken cancellationToken)
    {
        var aulaConcluidaDtos = await _alunoService.ObterAulasConcluidasPorMatriculaIdAsync(matriculaId, cancellationToken);
        return Ok(aulaConcluidaDtos);
    }

    [HttpPost("matriculas/{matriculaId:guid}/aulas/{aulaId:guid}/concluir")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ConcluirAula(Guid matriculaId, Guid aulaId,
        CancellationToken cancellationToken)
    {
        var result = await _alunoService.ConcluirAulaAsync(matriculaId, aulaId, cancellationToken);
        if (result)
        {
            return StatusCode(StatusCodes.Status201Created);
        }

        return StatusCode(StatusCodes.Status400BadRequest);
    }

    [HttpPost("matriculas/{matriculaId:guid}/curso/{cursoId:guid}/concluir")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SolicitarConclusaoCurso(
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
        var result = await _alunoService.SolicitarConclusaoCursoAsync(matriculaId, totalAulas, cancellationToken);

        if (!result)
        {
            return BadRequest(new { message = "Não foi possível concluir o curso. Verifique as pendências." });
        }

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpPost("{id:guid}/matriculas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AdicionarMatricula(Guid id, AdicionarMatriculaRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        if (id != requestDto.AlunoId) return BadRequest();
        var curso = await _conteudoService.ObterPorId(requestDto.CursoId, cancellationToken);
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

        var result = await _alunoService.AdicionarMatriculaAsync(id, matriculaCompletaDto, cancellationToken);
        if (result)
        {
            return StatusCode(StatusCodes.Status201Created);
        }

        return StatusCode(StatusCodes.Status400BadRequest);
    }
}