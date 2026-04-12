using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Alunos.API.Dtos;
using TelesEducacao.Alunos.Application.Commands;
using TelesEducacao.Alunos.Application.Queries;
using TelesEducacao.Alunos.Application.Queries.Dtos;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;
using MainController = TelesEducacao.WebAPI.Core.Controllers.MainController;

namespace TelesEducacao.Alunos.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Aluno")]
public class AlunosController : MainController
{
    private readonly IAlunoQueries _alunoQueries;

    private readonly IMediatorHandler _mediatorHandler;

    public AlunosController(INotificationHandler<DomainNotification> notifications, IMediatorHandler mediatorHandler,
        IAlunoQueries alunoQueries) : base(mediatorHandler, notifications)
    {
        _mediatorHandler = mediatorHandler;
        _alunoQueries = alunoQueries;
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AlunoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlunoDto>> ObterPorId(Guid id,
        CancellationToken cancellationToken)
    {
        var aluno = await _alunoQueries.ObterPorIdAsync(id, cancellationToken);
        if (aluno == null) return NotFound();
        return Ok(aluno);
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AlunoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> ObterTodos(
        CancellationToken cancellationToken)
    {
        var alunos = await _alunoQueries.ObterTodosAsync(cancellationToken);
        return Ok(alunos);
    }

    [HttpGet("{alunoId:guid}/matriculas")]
    [ProducesResponseType(typeof(IEnumerable<MatriculaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> ObterMatriculasPorAlunoId(Guid alunoId,
        CancellationToken cancellationToken)
    {
        var matriculaDtos = await _alunoQueries.ObterMatriculasPorAlunoIdAsync(alunoId, cancellationToken);
        return Ok(matriculaDtos);
    }

    [HttpGet("matriculas/{matriculaId:guid}")]
    [ProducesResponseType(typeof(MatriculaDto), StatusCodes.Status200OK)]
    public async Task<ActionResult> ObterMatriculaPorId(Guid matriculaId,
      CancellationToken cancellationToken)
    {
        var matriculaDto = await _alunoQueries.ObterMatriculaPorIdAsync(matriculaId, cancellationToken);
        return Ok(matriculaDto);
    }

    [HttpGet("matriculas/{matriculaId:guid}/aulas-concluidas")]
    [ProducesResponseType(typeof(IEnumerable<AulaConcluidaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> ObterAulasConcluidasPorMatriculaId(Guid matriculaId,
      CancellationToken cancellationToken)
    {
        var aulaConcluidaDtos = await _alunoQueries.ObterAulasConcluidasPorMatriculaIdAsync(matriculaId, cancellationToken);
        return Ok(aulaConcluidaDtos);
    }

    [HttpPost("{id:guid}/matriculas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AdicionarMatricula(Guid id, AdicionarMatriculaDto matriculaDto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id != matriculaDto.AlunoId) return BadRequest();

            var command = new AdicionarMatriculaCommand(
                matriculaDto.AlunoId,
                matriculaDto.CursoId,
                matriculaDto.Valor,
                matriculaDto.NomeCartao,
                matriculaDto.NumeroCartao,
                matriculaDto.ExpiracaoCartao,
                matriculaDto.CvvCartao
            );

            await _mediatorHandler.EnviarComando(command);

            if (OperacaoValida()) return StatusCode(StatusCodes.Status201Created);

            var erro = ObterMensagemErro();

            return BadRequest(new { message = erro });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("matriculas/{matriculaId:guid}/aulas/{aulaId:guid}/concluir")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ConcluirAula(Guid matriculaId, Guid aulaId,
        CancellationToken cancellationToken)
    {
        var command = new ConluirAulaCommand(aulaId, matriculaId);
        await _mediatorHandler.EnviarComando(command);
        if (OperacaoValida()) return StatusCode(StatusCodes.Status201Created);
        var erro = ObterMensagemErro();
        return BadRequest(new { message = erro });
    }

    [HttpPost("matriculas/{matriculaId:guid}/concluir")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SolicitarConclusaoCurso(Guid matriculaId, [FromBody] int totalAulasCurso, CancellationToken cancellationToken)
    {
        var command = new ConcluirCursoCommand(matriculaId, totalAulasCurso);

        await _mediatorHandler.EnviarComando(command);

        if (OperacaoValida())
            return StatusCode(StatusCodes.Status201Created);

        return BadRequest(new { message = ObterMensagemErro() });
    }
}