using MediatR;
using TelesEducacao.Conteudos.Application.Commands;
using TelesEducacao.Conteudos.Application.Events;
using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;
using TelesEducacao.MessageBus;

namespace TelesEducacao.Conteudo.API.Services;

public class ConteudoIntegrationHandler : BackgroundService
{
	private readonly IServiceProvider _serviceProvider;
	private readonly IMessageBus _bus;
	private IDisposable? _cursoResponder;
	private IDisposable? _aulaResponder;

	public ConteudoIntegrationHandler(IServiceProvider serviceProvider, IMessageBus bus)
	{
		_serviceProvider = serviceProvider;
		_bus = bus;
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// register RPC responders for CursoCriado and AulaCriada events
		_cursoResponder = _bus.RespondAsync<CursoCriadoIntegrationEvent, ResponseMessage>(async request =>
		{
			using var scope = _serviceProvider_CreateScope();
			var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

			try
			{
				Console.WriteLine("Mensagem de curso recebida");

				var cursoCommand = new CriarCursoCommand(
					request.Nome,
					request.Descricao,
					request.Ativo,
					request.Valor,
					request.ConteudoProgramaticoTitulo,
					request.ConteudoProgramaticoDescricao
				);

				var result = await mediator.Send(cursoCommand);

				Console.WriteLine("Handler de curso executado");

				if (!result)
					return new ResponseMessage(false, "Erro ao criar um curso");

				return new ResponseMessage(true);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Erro no handler de curso: {ex.Message}");
				throw;
			}
		});

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
		_cursoResponder?.Dispose();
		_aulaResponder?.Dispose();
		return base.StopAsync(cancellationToken);
	}
}
