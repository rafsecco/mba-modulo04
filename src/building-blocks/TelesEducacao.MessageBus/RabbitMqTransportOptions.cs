namespace TelesEducacao.MessageBus;

public class RabbitMqTransportOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string? User { get; set; }
    public string? Pass { get; set; }
    public string? VirtualHost { get; set; }
    public int Timeout { get; set; } = 30;
    public bool PublisherConfirms { get; set; } = true;
    public bool UseSsl { get; set; } = false;
}
