namespace TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;

public class UsuarioRegistradoIntegrationEvent : IntegrationEvent
{
    public Guid Id { get; private set; }

    public UsuarioRegistradoIntegrationEvent(Guid id)
    {
        Id = id;
    }
}