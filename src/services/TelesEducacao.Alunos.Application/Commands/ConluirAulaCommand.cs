using MediatR;
using TelesEducacao.Alunos.Domain;
using TelesEducacao.Core.Messages;

namespace TelesEducacao.Alunos.Application.Commands;

public class ConluirAulaCommand : Command, IRequest
{
    public Guid AulaId { get; init; }
    public Guid MatriculaId { get; init; }

    public ConluirAulaCommand(Guid aulaId, Guid matriculaId)
    {
        AulaId = aulaId;
        MatriculaId = matriculaId;
    }
}

public class ConcluirAulaCommandHandler : IRequestHandler<ConluirAulaCommand, bool>
{
    private readonly IAlunoRepository _repository;

    public ConcluirAulaCommandHandler(IAlunoRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(ConluirAulaCommand request, CancellationToken cancellationToken)
    {
        await _repository.ConcluirAula(request.MatriculaId, request.AulaId);
        return await _repository.UnitOfWork.Commit();
    }
}