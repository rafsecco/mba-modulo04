using TelesEducacao.Core.Messages;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;

namespace TelesEducacao.Core.Communication.Mediator;

public interface IMediatorHandler
{
    Task PublicarEvento<T>(T evento) where T : Event;

    Task EnviarComando<T>(T command) where T : Command;

    Task<bool> EnviarComandoAsync<T>(T command) where T : Command;

    Task PublicarNotificacao<T>(T notificacao) where T : DomainNotification;
}