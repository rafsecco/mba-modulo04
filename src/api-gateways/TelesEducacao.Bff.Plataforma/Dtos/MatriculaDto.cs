namespace TelesEducacao.Bff.Plataforma.Dtos;

public class MatriculaDto
{
    public Guid Id { get; init; }
    public Guid CursoId { get; init; }

    public MatriculaStatus MatriculaStatus { get; init; }
}

public enum MatriculaStatus
{
    Ativa,
    PendentePagamento,
    Cancelada,
    Concluida,
}