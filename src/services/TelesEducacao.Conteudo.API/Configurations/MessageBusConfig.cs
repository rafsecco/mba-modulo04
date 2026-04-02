using TelesEducacao.Conteudo.API.Services;
using TelesEducacao.Core.Utils;
using TelesEducacao.MessageBus;

namespace TelesEducacao.Conteudo.API.Configurations;

public static class MessageBusConfig
{
	public static void AddMessageBusConfiguration(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddMessageBus(configuration.GetMessageQueueConnection())
				.AddHostedService<ConteudoIntegrationHandler>();
	}
}
