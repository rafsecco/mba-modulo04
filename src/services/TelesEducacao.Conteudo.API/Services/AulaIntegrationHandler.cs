using MediatR;
using TelesEducacao.Conteudos.Application.Commands;
using TelesEducacao.Conteudos.Application.Events;
using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;
using TelesEducacao.MessageBus;

namespace TelesEducacao.Conteudo.API.Services;

public class AulaIntegrationHandler : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IMessageBus _bus;
	private IDisposable? _aulaResponder;

	public AulaIntegrationHandler(IServiceProvider serviceProvider, IMessageBus bus)
	{
		_serviceProvider = serviceProvider;
		_bus = bus;
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_aulaResponder = _bus.RespondAsync<AulaCriadaIntegrationEvent, ResponseMessage>(async request =>
		{
			using var scope = _serviceProvider_CreateScope();
			var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

			try
			{
				Console.WriteLine("Mensagem de aula recebida");

				var aulaCommand = new CriarAulaCommand(
					request.Titulo,
					request.Conteudo,
					request.CursoId
				);

				var result = await mediator.Send(aulaCommand);

				Console.WriteLine("Handler de aula executado");

				if (!result)
					return new ResponseMessage(false, "Erro ao criar a aula");

				return new ResponseMessage(true);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Erro no handler de aula: {ex.Message}");
				throw;
			}
		});

		return Task.CompletedTask;
	}

	private IServiceScope _serviceProvider_CreateScope()
	{
		return _serviceProvider.CreateScope();
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		_aulaResponder?.Dispose();
		return base.StopAsync(cancellationToken);
	}
}
