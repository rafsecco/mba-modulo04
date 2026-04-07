namespace TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;

public class PagamentoRealizadoIntegrationEvent : IntegrationEvent
{
    public Guid MatriculaId { get; private set; }
    public Guid AlunoId { get; private set; }
    public Guid PagamentoId { get; private set; }
    public Guid TransacaoId { get; private set; }
    public decimal Total { get; private set; }

    public PagamentoRealizadoIntegrationEvent(Guid matriculaId, Guid alunoId, Guid pagamentoId, Guid transacaoId, decimal total)
    {
        AggregateId = pagamentoId;
        MatriculaId = matriculaId;
        AlunoId = alunoId;
        PagamentoId = pagamentoId;
        TransacaoId = transacaoId;
        Total = total;
    }
}