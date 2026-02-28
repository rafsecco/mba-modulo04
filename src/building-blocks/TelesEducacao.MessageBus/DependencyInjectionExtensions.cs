using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TelesEducacao.MessageBus;

public static class DependencyInjectionExtensions
{
	public static void AddMessageBus(this IServiceCollection services, IConfiguration configuration, params Assembly[] consumerAssemblies)
	{
		services.AddOptions<RabbitMqTransportOptions>()
			.Bind(configuration.GetSection(nameof(RabbitMqTransportOptions)))
			.ValidateOnStart();

		services.AddMassTransit(busRegistration =>
		{
			//busRegistration.ConfigureHealthCheckOptions(options =>
			//{
			//	options.Name = "RabbitMQ";
			//	options.Tags.Add("infra");
			//});
			busRegistration.SetRabbitMqReplyToRequestClientFactory();
			busRegistration.AddConsumers(consumerAssemblies);
			busRegistration.UsingRabbitMq((busContext, busConfiguration) =>
			{
				busConfiguration.ConfigureEndpoints(busContext);
			});
		});
	}
}
