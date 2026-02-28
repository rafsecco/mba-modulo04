using MassTransit;
using MediatR;
using TelesEducacao.Conteudos.Application.Commands;
using TelesEducacao.Conteudos.Application.Events;
using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;

namespace TelesEducacao.Conteudo.API.Services;

public class ConteudoIntegrationHandler : IConsumer<CursoCriadoIntegrationEvent>
{
	private readonly IMediator _mediator;

	public ConteudoIntegrationHandler(IMediator mediator)
	{
		_mediator = mediator;
	}

	public async Task Consume(ConsumeContext<CursoCriadoIntegrationEvent> context)
	{
		try
		{
			Console.WriteLine("Mensagem recebida");
			var cursoCommand = new CriarCursoCommand(
				context.Message.Nome,
				context.Message.Descricao,
				context.Message.Ativo,
				context.Message.Valor,
				context.Message.ConteudoProgramaticoTitulo,
				context.Message.ConteudoProgramaticoDescricao
			);
			var result = await _mediator.Send(cursoCommand);

			Console.WriteLine("Handler executado");

			if (!result)
			{
				await context.RespondAsync(new ResponseMessage(false, "Erro ao criar um curso"));
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
