using FluentValidation.Results;
using MediatR;
using TelesEducacao.Conteudos.Domain;

namespace TelesEducacao.Conteudos.Application.Commands;

public class CriarCursoCommandHandler : IRequestHandler<CriarCursoCommand, bool>
{
	private readonly ICursoRepository _cursoRepository;

	public CriarCursoCommandHandler(ICursoRepository cursoRepository)
	{
		_cursoRepository = cursoRepository;
	}

	public async Task<bool> Handle(
		CriarCursoCommand request,
		CancellationToken cancellationToken)
	{
		var validationResult = new ValidationResult();

		if (string.IsNullOrEmpty(request.Nome))
		{
			validationResult.Errors.Add(new ValidationFailure("Nome", "Nome é obrigatório"));
		}

		if (string.IsNullOrEmpty(request.Descricao))
		{
			validationResult.Errors.Add(new ValidationFailure("Descricao", "Descrição é obrigatório"));
		}

		if (string.IsNullOrEmpty(request.ConteudoProgramaticoTitulo))
		{
			validationResult.Errors.Add(new ValidationFailure("ConteudoProgramaticoTitulo", "Conteudo Programático - Título é obrigatório"));
		}

		if (string.IsNullOrEmpty(request.ConteudoProgramaticoDescricao))
		{
			validationResult.Errors.Add(new ValidationFailure("ConteudoProgramaticoDescricao", "Conteudo Programático - Descrição é obrigatório"));
		}

		var curso = new Curso(
			request.Nome,
			request.Descricao,
			request.Ativo,
			request.Valor,
			new ConteudoProgramatico(
				request.ConteudoProgramaticoTitulo,
				request.ConteudoProgramaticoDescricao));

		_cursoRepository.Adicionar(curso);
		var result = await _cursoRepository.UnitOfWork.Commit();

		return !validationResult.Errors.Any();
	}
}
