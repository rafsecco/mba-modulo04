using TelesEducacao.Alunos.Application.Queries.Dtos;

namespace TelesEducacao.Alunos.Application.Queries;

public interface IAlunoQueries
{
    Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken cancellationToken);

    Task<AlunoDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<MatriculaDto>> ObterMatriculasPorAlunoIdAsync(Guid alunoId, CancellationToken cancellationToken);

    Task<MatriculaDto> ObterMatriculaPorIdAsync(Guid matriculaId, CancellationToken cancellationToken);

    Task<MatriculaDto> ObterMatriculaPorAlunoIdCursoId(Guid alunoId, Guid cursoId);

    Task<IEnumerable<AulaConcluidaDto>> ObterAulasConcluidasPorMatriculaIdAsync(Guid matriculaId, CancellationToken cancellationToken);

    Task<int> ObterTotalAulasConcluidasPorMatriculaId(Guid matriculaId);
}