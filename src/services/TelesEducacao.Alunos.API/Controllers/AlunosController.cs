using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Alunos.API.Dtos;
using TelesEducacao.Alunos.Application.Commands;
using TelesEducacao.Alunos.Application.Queries;
using TelesEducacao.Alunos.Application.Queries.Dtos;
using TelesEducacao.Alunos.Domain;
using TelesEducacao.Conteudos.Application.Services;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;
using ControllerBase = TelesEducacao.WebAPI.Core.Controllers.ControllerBase;

namespace TelesEducacao.Alunos.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Aluno")]
public class AlunosController : ControllerBase
{
    private readonly IAlunoQueries _alunoQueries;

    //TODO: Acessar de outra forma, sem depender do cursoAppService
    private readonly ICursoAppService _cursoAppService;

    private readonly IMediatorHandler _mediatorHandler;

    public AlunosController(INotificationHandler<DomainNotification> notifications, IMediatorHandler mediatorHandler,
        IAlunoQueries alunoQueries,
        ICursoAppService cursoAppService
    ) : base(mediatorHandler, notifications)
    {
        _mediatorHandler = mediatorHandler;
        _alunoQueries = alunoQueries;
        _cursoAppService = cursoAppService;
    }

    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AlunoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlunoDto>> ObterPorId(Guid id,
        CancellationToken cancellationToken)
    {
        var aluno = await _alunoQueries.ObterPorId(id);
        if (aluno == null) return NotFound();
        return Ok(aluno);
    }

    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AlunoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> ObterTodos(
        CancellationToken cancellationToken)
    {
        var alunos = await _alunoQueries.ObterTodos();
        return Ok(alunos);
    }

    [HttpGet("{id}/Matriculas")]
    [ProducesResponseType(typeof(IEnumerable<MatriculaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> ObterMatriculasPorAlunoId(Guid id,
        CancellationToken cancellationToken)
    {
        var matriculaDtos = await _alunoQueries.ObterMatriculasPorAlunoId(id);
        return Ok(matriculaDtos);
    }

    [HttpPost("{id}/Matricula/{cursoId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AdicionarMatricula(Guid id, Guid cursoId, AdicionarMatriculaDto matriculaDto,
        CancellationToken cancellationToken)
    {
        try
        {
            if (id != matriculaDto.AlunoId || cursoId != matriculaDto.CursoId) return BadRequest();

            var curso = await _cursoAppService.ObterPorId(cursoId);

            if (curso == null) return BadRequest(new { message = "Curso não encontrado." });

            var command = new AdicionarMatriculaCommand(
                matriculaDto.AlunoId,
                matriculaDto.CursoId,
                curso.Valor,
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

    [HttpPost("{id}/Matricula/{aulaId}/AulasConcluidas")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ConcluirAula(Guid id, Guid aulaId,
        CancellationToken cancellationToken)
    {
        //verificar se a aula é do curso que o aluno está matriculado
        var command = new ConluirAulaCommand(id, aulaId);
        await _mediatorHandler.EnviarComando(command);
        if (OperacaoValida()) return StatusCode(StatusCodes.Status201Created);
        var erro = ObterMensagemErro();
        return BadRequest(new { message = erro });
    }

    [HttpPost("{id}/Matricula/Certificados")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SolicitarFinalizacaoCurso(Guid id,
        CancellationToken cancellationToken)
    {
        var matricula = await _alunoQueries.ObterMatriculaPorId(id);

        if (matricula == null || matricula.MatriculaStatus != MatriculaStatus.Ativa)
            return BadRequest(new { message = "Matrícula não encontrada." });

        var aulasCurso = await _cursoAppService.ObterAulas(matricula.CursoId);

        if (aulasCurso == null || !aulasCurso.Any())
            return BadRequest(new { message = "Este curso não possui aulas cadastradas." });

        var aulasConcluidas = await _alunoQueries.ObterAulasConcluidasPorMatriculaId(matricula.Id);

        var totalAulasCurso = aulasCurso.Count();
        var totalConcluidas = aulasConcluidas.Count();

        if (totalConcluidas < totalAulasCurso)
            return BadRequest(new
            {
                message =
                    $"Não é possível concluir o curso. Você concluiu {totalConcluidas} de {totalAulasCurso} aulas."
            });

        var command = new ConcluirCursoCommand(matricula.Id);
        await _mediatorHandler.EnviarComando(command);
        if (OperacaoValida()) return StatusCode(StatusCodes.Status201Created);
        var erro = ObterMensagemErro();
        return BadRequest(new { message = erro });
    }
}