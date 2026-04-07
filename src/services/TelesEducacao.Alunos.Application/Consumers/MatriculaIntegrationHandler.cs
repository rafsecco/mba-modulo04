using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelesEducacao.Alunos.Application.Commands;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;
using TelesEducacao.MessageBus;

namespace TelesEducacao.Alunos.Application.Consumers;

public class MatriculaIntegrationHandler : BackgroundService
{
    private readonly IMessageBus _messageBus;
    private readonly IServiceProvider _serviceProvider;

    public MatriculaIntegrationHandler(IMessageBus messageBus, IServiceProvider serviceProvider)
    {
        _messageBus = messageBus;
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _messageBus.SubscribeAsync<PagamentoRealizadoIntegrationEvent>("PagamentoRealizado", async request =>
          await HandlePagamentoRealizadoAsync(request, stoppingToken));

        _messageBus.SubscribeAsync<PagamentoRecusadoIntegrationEvent>("PagamentoRecusado", async request =>
        await HandlePagamentoResucasadoAsync(request, stoppingToken));
        return Task.CompletedTask;
    }

    public async Task HandlePagamentoRealizadoAsync(PagamentoRealizadoIntegrationEvent notification, CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var mediatorHandler = scope.ServiceProvider.GetRequiredService<IMediatorHandler>();
            await mediatorHandler.EnviarComando(new AtivarMatriculaCommand(notification.MatriculaId));
        }
    }

    public async Task HandlePagamentoResucasadoAsync(PagamentoRecusadoIntegrationEvent notification, CancellationToken cancellationToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var mediatorHandler = scope.ServiceProvider.GetRequiredService<IMediatorHandler>();
            await mediatorHandler.EnviarComando(new CancelarMatriculaCommand(notification.MatriculaId));
        }
    }
}