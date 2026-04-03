using MediatR;
using TelesEducacao.Core.Messages;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;

namespace TelesEducacao.Core.Communication.Mediator;

public class MediatorHandler : IMediatorHandler
{
    private readonly IMediator _mediator;

    public MediatorHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task PublicarEvento<T>(T evento) where T : Event
    {
        await _mediator.Publish(evento);
    }

    public async Task EnviarComando<T>(T command) where T : Command
    {
        await _mediator.Send(command);
    }

    public async Task PublicarNotificacao<T>(T notificacao) where T : DomainNotification
    {
        await _mediator.Publish(notificacao);
    }

    public async Task<bool> EnviarComandoAsync<T>(T command) where T : Command
    {
        return await _mediator.Send(command);
    }
}