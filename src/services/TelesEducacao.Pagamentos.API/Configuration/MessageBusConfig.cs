using TelesEducacao.Core.Utils;
using TelesEducacao.MessageBus;
using TelesEducacao.Pagamentos.Business.Consumers;

namespace TelesEducacao.Pagamentos.API.Configuration;

public static class MessageBusConfig
{
    public static void AddMessageBusConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMessageBus(configuration.GetMessageQueueConnection("MessageBus"));
        services.AddHostedService<PagamentoEventHandler>();
    }
}