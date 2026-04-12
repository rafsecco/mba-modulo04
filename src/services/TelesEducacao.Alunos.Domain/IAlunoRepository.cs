using TelesEducacao.Core.Data;

namespace TelesEducacao.Alunos.Domain;

public interface IAlunoRepository : IRepository<Aluno>
{
    void CriarAsync(Aluno aluno);

    Task<Aluno?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<Aluno>> ObterTodosAsync(CancellationToken cancellationToken);

    Task<Guid?> AdicionarMatriculaAsync(Guid alunoId, Guid cursoId);

    Task<IEnumerable<Matricula>> ObterMatriculasPorAlunoIdAsync(Guid alunoId, CancellationToken cancellationToken);

    Task AlterarStatusMatriculaAsync(Guid matriculaId, MatriculaStatus status);

    Task ConcluirAula(Guid matriculaId, Guid aulaId);

    Task<Matricula> ObterMatriculaPorIdAsync(Guid matriculaId, CancellationToken cancellationToken);

    Task<IEnumerable<AulaConluida>> ObterAulasConcluidasPorMatriculaIdAsync(Guid matriculaId, CancellationToken cancellationToken);

    Task<Guid?> AdicionarCertificadoAsync(Guid matriculaId);

    Task<Matricula?> ObterMatriculaPorAlunoIdCursoId(Guid alunoId, Guid cursoId);

    Task<int> ContarAulasConcluidasPorMatriculaId(Guid matriculaId);
}