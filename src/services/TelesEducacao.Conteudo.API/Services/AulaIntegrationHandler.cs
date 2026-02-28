using MassTransit;
using MediatR;
using TelesEducacao.Conteudos.Application.Commands;
using TelesEducacao.Conteudos.Application.Events;
using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;

namespace TelesEducacao.Conteudo.API.Services;

public class AulaIntegrationHandler : IConsumer<AulaCriadaIntegrationEvent>
{
	private readonly IMediator _mediator;

	public AulaIntegrationHandler(IMediator mediator)
	{
		_mediator = mediator;
	}

	public async Task Consume(ConsumeContext<AulaCriadaIntegrationEvent> context)
	{
		try
		{
			Console.WriteLine("Mensagem recebida");
			var aulaCommand = new CriarAulaCommand(
				context.Message.Titulo,
				context.Message.Conteudo,
				context.Message.CursoId
			);
			var result = await _mediator.Send(aulaCommand);

			Console.WriteLine("Handler executado");

			if (!result)
			{
				await context.RespondAsync(new ResponseMessage(false, "Erro ao criar a aula"));
				return;
			}

			await context.RespondAsync(new ResponseMessage(true));
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Erro no consumer: {ex.Message}");
			throw;
		}
	}
}
