using Microsoft.Extensions.Configuration;

namespace TelesEducacao.Core.Utils;

public static class ConfigurationExtensions
{
	public static string GetMessageQueueConnection(this IConfiguration configuration)
	{
		var section = configuration?.GetSection("RabbitMqTransportOptions");
		if (section != null && section.Exists())
		{
			var host = section["Host"] ?? "localhost";
			var port = section["Port"] ?? "5672";
			var user = section["User"];
			var pass = section["Pass"];
			var virtualHost = section["VirtualHost"];
			var timeout = section["Timeout"] ?? "30";
			var publisherConfirms = section["PublisherConfirms"] ?? "true";
			var useSsl = section["UseSsl"] ?? "false";

			var sb = new System.Text.StringBuilder();
			sb.Append($"host={host}:{port};");
			if (!string.IsNullOrWhiteSpace(user)) sb.Append($"username={user};");
			if (!string.IsNullOrWhiteSpace(pass)) sb.Append($"password={pass};");
			if (!string.IsNullOrWhiteSpace(virtualHost)) sb.Append($"virtualHost={virtualHost};");
			if (!string.IsNullOrWhiteSpace(publisherConfirms)) sb.Append($"publisherConfirms={publisherConfirms};");
			if (!string.IsNullOrWhiteSpace(timeout)) sb.Append($"timeout={timeout};");
			if (!string.IsNullOrWhiteSpace(useSsl) && bool.TryParse(useSsl, out var ssl) && ssl) sb.Append($"ssl=true;");

			return sb.ToString().TrimEnd(';');
		}

		return string.Empty;
	}
}
