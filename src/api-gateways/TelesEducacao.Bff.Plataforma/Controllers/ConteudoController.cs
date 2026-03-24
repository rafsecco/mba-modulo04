using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Bff.Plataforma.Services;
using TelesEducacao.Conteudos.Application.Dtos;

namespace TelesEducacao.Bff.Plataforma.Controllers;

[Authorize]
[Route("[controller]")]
public class ConteudoController : MainController
{
	private readonly IConteudoService _conteudoService;

	public ConteudoController(IConteudoService conteudoService)
	{
		_conteudoService = conteudoService;
	}

	[AllowAnonymous]
	[HttpGet]
	[ProducesResponseType(typeof(List<CursoDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status502BadGateway)]
	public async Task<ActionResult> ObterTodos(CancellationToken cancellationToken)
	{
		var cursos = await _conteudoService.ObterTodos(cancellationToken);
		return Ok(cursos);
	}

	[AllowAnonymous]
	[HttpGet("{id}")]
	[ProducesResponseType(typeof(CursoDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status502BadGateway)]
	public async Task<ActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
	{
		var curso = await _conteudoService.ObterPorId(id, cancellationToken);
		if (curso == null) return NotFound(new { message = "Curso não encontrado." });
		return Ok(curso);
	}

	[AllowAnonymous]
	[HttpGet("{cursoId}/aulas")]
	[ProducesResponseType(typeof(List<AulaDto>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status502BadGateway)]
	public async Task<ActionResult> ObterAulasPorCurso(Guid cursoId, CancellationToken cancellationToken)
	{
		var aulas = await _conteudoService.ObterAulasPorCurso(cursoId, cancellationToken);
		return Ok(aulas);
	}

	[AllowAnonymous]
	[HttpGet("{cursoId}/aulas/{aulaId}")]
	[ProducesResponseType(typeof(AulaDto), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status502BadGateway)]
	public async Task<ActionResult> ObterAula(Guid cursoId, Guid aulaId, CancellationToken cancellationToken)
	{
		var aula = await _conteudoService.ObterAula(cursoId, aulaId, cancellationToken);
		if (aula == null) return NotFound(new { message = "Aula não encontrada." });
		return Ok(aula);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> Cria([FromForm] CriaCursoDto criaCursoDto, CancellationToken cancellationToken)
	{
		if (!ModelState.IsValid) return CustomResponse(ModelState);

		var result = await _conteudoService.Cria(criaCursoDto, cancellationToken);
		if (result) return StatusCode(StatusCodes.Status201Created);

		return BadRequest(new { message = "Erro ao criar curso." });
	}

	[HttpPost("{cursoId}/aulas")]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult> CriarAula(Guid cursoId, [FromBody] CriaAulaDto dto, CancellationToken cancellationToken)
	{
		if (!ModelState.IsValid) return CustomResponse(ModelState);

		var result = await _conteudoService.CriarAula(cursoId, dto, cancellationToken);
		if (result) return StatusCode(StatusCodes.Status201Created);

		return BadRequest(new { message = "Erro ao criar aula." });
	}

	[HttpPut]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Atualiza([FromBody] AtualizaCursoDto atualizaCursoDto, CancellationToken cancellationToken)
	{
		if (!ModelState.IsValid) return CustomResponse(ModelState);

		var result = await _conteudoService.Atualiza(atualizaCursoDto, cancellationToken);
		if (result) return NoContent();

		return BadRequest(new { message = "Erro ao atualizar curso." });
	}

	[HttpDelete("{id}")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> Remove(Guid id, CancellationToken cancellationToken)
	{
		var result = await _conteudoService.Remove(id, cancellationToken);
		if (result) return NoContent();

		return BadRequest(new { message = "Erro ao remover curso." });
	}
}

