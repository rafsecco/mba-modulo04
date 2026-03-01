using System.Reflection;
using TelesEducacao.MessageBus;

namespace TelesEducacao.Conteudo.API.Configurations;

public static class MessageBusConfig
{
	public static void AddMessageBusConfiguration(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddMessageBus(configuration, Assembly.GetAssembly(typeof(Program)));
	}
}
