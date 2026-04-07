using MediatR;
using TelesEducacao.Alunos.Domain;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages;
using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;
using TelesEducacao.MessageBus;

namespace TelesEducacao.Alunos.Application.Commands;

public class AdicionarMatriculaCommandHandler : IRequestHandler<AdicionarMatriculaCommand, bool>
{
    private readonly IAlunoRepository _alunoRepository;
    private readonly IMediatorHandler _mediatorHandler;
    private readonly IMessageBus _messageBus;

    public AdicionarMatriculaCommandHandler(IAlunoRepository alunoRepository, IMediatorHandler mediatorHandler, IMessageBus messageBus)
    {
        _alunoRepository = alunoRepository;
        _mediatorHandler = mediatorHandler;
        _messageBus = messageBus;
    }

    public async Task<bool> Handle(AdicionarMatriculaCommand request, CancellationToken cancellationToken)
    {
        if (!ValidarComando(request)) return false;

        var matriculaId = await _alunoRepository.AdicionarMatriculaAsync(request.AlunoId, request.CursoId);
        var result = await _alunoRepository.UnitOfWork.Commit();

        if (matriculaId.HasValue)
        {
            var matriculaAdicionadaEvent = new MatriculaAdicionadaIntegrationEvent(
                matriculaId.Value,
                request.Valor,
                request.AlunoId,
                request.CursoId,
                request.NomeCartao,
                request.NumeroCartao,
                request.ExpiracaoCartao,
                request.CvvCartao
            );

            await _messageBus.PublishAsync<MatriculaAdicionadaIntegrationEvent>(matriculaAdicionadaEvent);
        }

        return result;
    }

    private bool ValidarComando(Command command)
    {
        if (command.EhValido()) return true;

        foreach (var error in command.ValidationResult.Errors)
        {
            _mediatorHandler.PublicarNotificacao(new DomainNotification(command.MessageType, error.ErrorMessage));
        }

        return false;
    }
}