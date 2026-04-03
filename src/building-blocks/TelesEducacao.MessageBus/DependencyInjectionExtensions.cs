using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;

namespace TelesEducacao.MessageBus;

public static class DependencyInjectionExtensions
{
    public static void AddMessageBus(this IServiceCollection services, string connectionString)
    {
        services.AddEasyNetQ(connectionString);

        services.AddSingleton<IMessageBus, MessageBus>();
    }
}