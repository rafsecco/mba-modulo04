using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Conteudos.Application.Dtos;
using TelesEducacao.Conteudos.Application.Services;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;

namespace TelesEducacao.Conteudo.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Admin")]
public class ConteudoController : TelesEducacao.WebAPI.Core.Controllers.ControllerBase
{
	private readonly ICursoAppService _cursoAppService;

	public ConteudoController(INotificationHandler<DomainNotification> notifications, IMediatorHandler mediatorHandler, ICursoAppService cursoAppService) : base(mediatorHandler, notifications)
	{
		_cursoAppService = cursoAppService;
	}

	[HttpPost]
	[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<Guid>> Cria([FromForm] CriaCursoDto criaCursoDto,
		CancellationToken cancellationToken)
	{
		try
		{
			var cursoId = await _cursoAppService.Adicionar(criaCursoDto);
			return StatusCode(StatusCodes.Status201Created, cursoId);
		}
		catch (UnauthorizedAccessException ex)
		{
			return Unauthorized(new { message = ex.Message });
		}
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

	[HttpPost("{cursoId}/aulas")]
	[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
	public async Task<ActionResult<Guid>> CriarAula(Guid cursoId, [FromBody] CriaAulaDto dto, CancellationToken ct)
	{
		dto.CursoId = cursoId;
		var aulaId = await _cursoAppService.AdicionarAula(dto);
		return StatusCode(StatusCodes.Status201Created, aulaId);
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
}
