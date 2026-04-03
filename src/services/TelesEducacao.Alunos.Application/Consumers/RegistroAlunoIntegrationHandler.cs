using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelesEducacao.Alunos.Application.Commands;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;
using TelesEducacao.MessageBus;

namespace TelesEducacao.Alunos.Application.Consumers;

public class RegistroAlunoIntegrationHandler : BackgroundService
{
    private readonly IMessageBus _messageBus;
    private readonly IServiceProvider _serviceProvider;

    public RegistroAlunoIntegrationHandler(IMessageBus messageBus, IServiceProvider serviceProvider)
    {
        _messageBus = messageBus;
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _messageBus.RespondAsync<UsuarioRegistradoIntegrationEvent, ResponseMessage>(async request =>
             await RegistrarCliente(request));
        return Task.CompletedTask;
    }

    private async Task<ResponseMessage> RegistrarCliente(UsuarioRegistradoIntegrationEvent message)
    {
        var clienteCommand = new CriarAlunoCommand(message.Id);
        bool result;

        using (var scope = _serviceProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediatorHandler>();
            result = await mediator.EnviarComandoAsync(clienteCommand);
        }

        if (!result)
        {
            return new ResponseMessage(false, "Erro ao criar a aluno"); ;
        }

        return new ResponseMessage(true);
    }
}