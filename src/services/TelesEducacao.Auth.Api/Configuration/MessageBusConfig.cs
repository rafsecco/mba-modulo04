using TelesEducacao.Core.Utils;
using TelesEducacao.MessageBus;

namespace TelesEducacao.Auth.API.Configuration;

public static class MessageBusConfig
{
    public static void AddMessageBusConfiguration(this IServiceCollection services,
    IConfiguration configuration)
    {
        services.AddMessageBus(configuration.GetMessageQueueConnection("MessageBus"));
    }
}