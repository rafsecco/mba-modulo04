using MassTransit;
using Microsoft.Extensions.Configuration;

namespace TelesEducacao.MessageBus;

public static class ConfigurationExtensions
{
	public static string GetMessagingConnectionString(this IConfiguration configuration)
	{
		var options = configuration
			.GetSection("RabbitMqTransportOptions")
			.Get<RabbitMqTransportOptions>();

		return
			$"host={options.Host}:{options.Port};publisherConfirms=true;timeout=30;username={options.User};password={options.Pass}";
	}
}
