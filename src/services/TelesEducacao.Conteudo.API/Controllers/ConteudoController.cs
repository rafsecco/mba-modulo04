using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Conteudos.Application.Dtos;
using TelesEducacao.Conteudos.Application.Events;
using TelesEducacao.Conteudos.Application.Services;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;

namespace TelesEducacao.Conteudo.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Admin")]
public class ConteudoController : TelesEducacao.WebAPI.Core.Controllers.MainController
{
	private readonly ICursoAppService _cursoAppService;
	private readonly IBus _bus;

	public ConteudoController(
		INotificationHandler<DomainNotification> notifications,
		IMediatorHandler mediatorHandler,
		ICursoAppService cursoAppService,
		IBus bus) : base(mediatorHandler, notifications)
	{
		_cursoAppService = cursoAppService;
		_bus = bus;
	}

	[HttpPost]
	[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult> Cria([FromForm] CriaCursoDto criaCursoDto, CancellationToken cancellationToken)
	{
		try
		{
			var response = await CriarCurso(criaCursoDto);
			return StatusCode(StatusCodes.Status201Created, response);
		}
		catch (UnauthorizedAccessException ex)
		{
			return Unauthorized(new { message = ex.Message });
		}
	}
	private async Task<ResponseMessage> CriarCurso(CriaCursoDto cursoDto)
	{
		var cursoCriado = new CursoCriadoIntegrationEvent(
			Guid.NewGuid(),
			cursoDto.Nome,
			cursoDto.Descricao,
			cursoDto.Ativo,
			cursoDto.Valor,
			cursoDto.ConteudoProgramatico.Titulo,
			cursoDto.ConteudoProgramatico.Descricao);
		try
		{
			var response = await _bus.Request<CursoCriadoIntegrationEvent, ResponseMessage>(cursoCriado);
			return response.Message;
		}
		catch (Exception)
		{
			throw;
		}
	}


	[HttpPost("{cursoId}/aulas")]
	[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
	public async Task<ActionResult<Guid>> CriarAula(Guid cursoId, [FromBody] CriaAulaDto dto, CancellationToken ct)
	{
		dto.CursoId = cursoId;
		var response = await AdicionarAula(dto);
		return StatusCode(StatusCodes.Status201Created, response);
	}
	private async Task<ResponseMessage> AdicionarAula(CriaAulaDto aulaDto)
	{
		var aulaCriada = new AulaCriadaIntegrationEvent(
			aulaDto.Titulo,
			aulaDto.Conteudo,
			aulaDto.CursoId);
		try
		{
			Response<ResponseMessage>? response = await _bus.Request<AulaCriadaIntegrationEvent, ResponseMessage>(aulaCriada);
			return response.Message;
		}
		catch (Exception)
		{
			throw;
		}
	}


	[AllowAnonymous]
	[HttpGet]
	[ProducesResponseType(typeof(List<CursoDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<List<CursoDto>>> ObterTodos(CancellationToken cancellationToken)
	{
		var result = await _cursoAppService.ObterTodos();
		return Ok(result);
	}

	[AllowAnonymous]
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(CursoDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<CursoDto>> ObterPorId(Guid id, CancellationToken cancellationToken)
	{
		var cursoDto = await _cursoAppService.ObterPorId(id);

		if (cursoDto == null)
			return NotFound();

		return Ok(cursoDto);
	}

	[AllowAnonymous]
	[HttpGet("{cursoId}/aulas")]
	[ProducesResponseType(typeof(List<AulaDto>), StatusCodes.Status200OK)]
	public async Task<ActionResult<List<AulaDto>>> ObterAulasPorCurso(Guid cursoId, CancellationToken ct)
	{
		var aulas = await _cursoAppService.ObterAulas(cursoId);
		return Ok(aulas);
	}

	[AllowAnonymous]
	[HttpGet("{cursoId}/aulas/{aulaId}")]
	[ProducesResponseType(typeof(AulaDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<AulaDto>> ObterAula(Guid cursoId, Guid aulaId, CancellationToken ct)
	{
		var aula = await _cursoAppService.ObterAula(aulaId);
		if (aula == null) return NotFound();
		return Ok(aula);
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<IActionResult> Atualiza([FromBody] AtualizaCursoDto atualizaCursoDto,
		CancellationToken cancellationToken)
	{
		try
		{
			await _cursoAppService.Atualizar(atualizaCursoDto);
			return NoContent();
		}
		catch (UnauthorizedAccessException ex)
		{
			return Unauthorized(new { message = ex.Message });
		}
		catch (KeyNotFoundException ex)
		{
			return NotFound(new { message = ex.Message });
		}
	}

	[HttpDelete]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
	{
		try
		{
			await _cursoAppService.Remover(id);
			return NoContent();
		}
		catch (UnauthorizedAccessException ex)
		{
			return Unauthorized(new { message = ex.Message });
		}
	}
}
