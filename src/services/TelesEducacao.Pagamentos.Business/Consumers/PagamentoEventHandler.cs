using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelesEducacao.Core.DomainObjects;
using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;
using TelesEducacao.MessageBus;

namespace TelesEducacao.Pagamentos.Business.Consumers;

public class PagamentoEventHandler : BackgroundService
{
    private readonly IMessageBus _messageBus;
    private readonly IServiceProvider _serviceProvider;

    public PagamentoEventHandler(IMessageBus messageBus, IServiceProvider serviceProvider)
    {
        _messageBus = messageBus;
        _serviceProvider = serviceProvider;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _messageBus.SubscribeAsync<MatriculaAdicionadaIntegrationEvent>("MatriculaAdicionada", async request =>
           await HandlePagamentoMatricula(request, stoppingToken));
        return Task.CompletedTask;
    }

    public async Task HandlePagamentoMatricula(MatriculaAdicionadaIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var pagamentoMatricula = new PagamentoMatricula
        {
            MatriculaId = notification.MatriculaId,
            AlunoId = notification.AlunoId,
            CursoId = notification.CursoId,
            NomeCartao = notification.NomeCartao,
            NumeroCartao = notification.NumeroCartao,
            ExpiracaoCartao = notification.ExpiracaoCartao,
            CvvCartao = notification.CvvCartao,
            Valor = notification.Valor
        };

        using (var scope = _serviceProvider.CreateScope())
        {
            var pagamentoService = scope.ServiceProvider.GetRequiredService<IPagamentoService>();
            await pagamentoService.RealizarPagamentoMatricula(pagamentoMatricula);
        }
    }
}