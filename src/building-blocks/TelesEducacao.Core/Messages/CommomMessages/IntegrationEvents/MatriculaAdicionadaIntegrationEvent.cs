namespace TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;

public class MatriculaAdicionadaIntegrationEvent : IntegrationEvent
{
    public Guid MatriculaId { get; init; }
    public Guid AlunoId { get; init; }

    public Guid CursoId { get; init; }

    public decimal Valor { get; init; }

    public string NomeCartao { get; init; }

    public string NumeroCartao { get; init; }

    public string ExpiracaoCartao { get; init; }

    public string CvvCartao { get; init; }

    public MatriculaAdicionadaIntegrationEvent(Guid matriculaId, decimal valor, Guid alunoId, Guid cursoId, string nomeCartao, string numeroCartao, string expiracaoCartao, string cvvCartao)
    {
        AlunoId = alunoId;
        CursoId = cursoId;
        NomeCartao = nomeCartao;
        NumeroCartao = numeroCartao;
        ExpiracaoCartao = expiracaoCartao;
        CvvCartao = cvvCartao;
        Valor = valor;
        MatriculaId = matriculaId;
    }
}