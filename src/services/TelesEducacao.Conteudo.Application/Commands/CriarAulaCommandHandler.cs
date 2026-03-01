using FluentValidation.Results;
using MediatR;
using TelesEducacao.Conteudos.Domain;

namespace TelesEducacao.Conteudos.Application.Commands;

public class CriarAulaCommandHandler : IRequestHandler<CriarAulaCommand, bool>
{
	private readonly ICursoRepository _cursoRepository;

	public CriarAulaCommandHandler(ICursoRepository cursoRepository)
	{
		_cursoRepository = cursoRepository;
	}

	public async Task<bool> Handle(
		CriarAulaCommand request,
		CancellationToken cancellationToken)
	{
		var validationResult = new ValidationResult();

		if (string.IsNullOrEmpty(request.Titulo))
		{
			validationResult.Errors.Add(new ValidationFailure("Titulo", "Ttulo é obrigatório"));
		}

		if (string.IsNullOrEmpty(request.Conteudo))
		{
			validationResult.Errors.Add(new ValidationFailure("Conteudo", "Conte´´udo é obrigatório"));
		}

		if (request.CursoId == Guid.Empty)
		{
			validationResult.Errors.Add(new ValidationFailure("CursoId", "Id do Curso é obrigatório"));
		}

		var aula = new Aula(
			request.Titulo,
			request.Conteudo,
			request.CursoId);

		_cursoRepository.AdicionarAula(aula);
		var result = await _cursoRepository.UnitOfWork.Commit();

		return !validationResult.Errors.Any();
	}
}
