using FluentValidation;
using MediatR;
using TelesEducacao.Alunos.Application.Queries;
using TelesEducacao.Alunos.Domain;
using TelesEducacao.Core.Messages;

namespace TelesEducacao.Alunos.Application.Commands;

public class ConcluirCursoCommand : Command, IRequest
{
    public Guid MatriculaId { get; init; }

    public int TotalAulasCurso { get; init; }

    public ConcluirCursoCommand(Guid matriculaId, int totalAulasCurso)
    {
        MatriculaId = matriculaId;
        TotalAulasCurso = totalAulasCurso;
    }
}

public class ConcluirCursoCommandValidator : AbstractValidator<ConcluirCursoCommand>
{
    private readonly IAlunoQueries _alunoQueries;

    public ConcluirCursoCommandValidator(IAlunoQueries alunoQueries)
    {
        _alunoQueries = alunoQueries;
        RuleFor(x => x.MatriculaId).NotEmpty();
        RuleFor(x => x.TotalAulasCurso).GreaterThan(0);
        RuleFor(x => x).MustAsync(async (command, cancellationToken) => await IsConclusaoValidaAsync(command));
    }

    public async Task<bool> IsConclusaoValidaAsync(ConcluirCursoCommand command)
    {
        if (command.MatriculaId == Guid.Empty) return false;

        var matricula = await _alunoQueries.ObterMatriculaPorId(command.MatriculaId);

        if (matricula.MatriculaStatus != MatriculaStatus.Ativa)
            return false;

        var totalAulasConcluidas = await _alunoQueries.ObterTotalAulasConcluidasPorMatriculaId(matricula.Id);

        if (totalAulasConcluidas < command.TotalAulasCurso)
            return false;

        return true;
    }
}

public class ConcluirCursoCommandHandler : IRequestHandler<ConcluirCursoCommand, bool>
{
    private readonly IAlunoRepository _alunoRepository;

    public ConcluirCursoCommandHandler(IAlunoRepository alunoRepository)
    {
        _alunoRepository = alunoRepository;
    }

    public async Task<bool> Handle(ConcluirCursoCommand request, CancellationToken cancellationToken)
    {
        await _alunoRepository.AlterarStatusMatriculaAsync(request.MatriculaId, MatriculaStatus.Concluida);

        await _alunoRepository.AdicionarCertificadoAsync(request.MatriculaId);
        return await _alunoRepository.UnitOfWork.Commit();
    }
}