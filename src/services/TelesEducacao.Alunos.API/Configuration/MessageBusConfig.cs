using TelesEducacao.Alunos.Application.Consumers;
using TelesEducacao.Core.Utils;
using TelesEducacao.MessageBus;

namespace TelesEducacao.Alunos.API.Configuration;

public static class MessageBusConfig
{
    public static void AddMessageBusConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMessageBus(configuration.GetMessageQueueConnection("MessageBus"));
        services.AddHostedService<RegistroAlunoIntegrationHandler>();
        services.AddHostedService<MatriculaIntegrationHandler>();
    }
}