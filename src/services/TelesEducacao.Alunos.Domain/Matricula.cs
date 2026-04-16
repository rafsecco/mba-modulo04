using TelesEducacao.Core.DomainObjects;

namespace TelesEducacao.Alunos.Domain;

public class Matricula : Entity
{
    public Guid AlunoId { get; private set; }
    public Aluno Aluno { get; private set; }
    public Guid CursoId { get; private set; }

    public MatriculaStatus? Status { get; private set; }

    public List<Certificado> Certificados { get; private set; }

    public Matricula(Guid alunoId, Guid cursoId)
    {
        AlunoId = alunoId;
        CursoId = cursoId;
        Status = MatriculaStatus.PendentePagamento;
    }

    protected Matricula()
    { }

    public void AtualizarStatus(MatriculaStatus novoStatus)
    {
        Status = novoStatus;
    }
}