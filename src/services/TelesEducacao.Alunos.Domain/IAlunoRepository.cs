using TelesEducacao.Core.Data;

namespace TelesEducacao.Alunos.Domain;

public interface IAlunoRepository : IRepository<Aluno>
{
    void CriarAsync(Aluno aluno);

    Task<Aluno?> ObterPorUserIdAsync(Guid userId);

    Task<IEnumerable<Aluno>> ObterTodosAsync();

    Task<Guid?> AdicionarMatriculaAsync(Guid alunoId, Guid cursoId);

    Task<IEnumerable<Matricula>> ObterMatriculasPorAlunoIdAsync(Guid alunoId);

    Task AlterarStatusMatriculaAsync(Guid matriculaId, MatriculaStatus status);

    Task ConcluirAula(Guid matriculaId, Guid aulaId);

    Task<Matricula> ObterMatriculaPorId(Guid matriculaId);

    Task<IEnumerable<AulaConluida>> ObterAulasConcluidasPorMatriculaId(Guid matriculaId);

    Task<Guid?> AdicionarCertificadoAsync(Guid matriculaId);

    Task<Matricula?> ObterMatriculaPorAlunoIdCursoId(Guid alunoId, Guid cursoId);
    Task<int> ContarAulasConcluidasPorMatriculaId(Guid matriculaId);
}