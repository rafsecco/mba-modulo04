using Microsoft.EntityFrameworkCore;
using TelesEducacao.Alunos.Domain;
using TelesEducacao.Core.Data;

namespace TelesEducacao.Alunos.Data.Repository;

public class AlunoRepository : IAlunoRepository
{
    private readonly AlunosContext _context;

    public AlunoRepository(AlunosContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public void CriarAsync(Aluno aluno)
    {
        _context.Alunos.Add(aluno);
    }

    public async Task<Aluno?> ObterPorUserIdAsync(Guid userId)
    {
        return await _context.Alunos.FirstOrDefaultAsync(a => a.UserId == userId);
    }

    public async Task<IEnumerable<Aluno>> ObterTodosAsync()
    {
        return await _context.Alunos.ToListAsync();
    }

    public async Task<Guid?> AdicionarMatriculaAsync(Guid alunoId, Guid cursoId)
    {
        var matricula = new Matricula(alunoId, cursoId);
        await _context.AddAsync(matricula);
        return matricula.Id;
    }

    public async Task<IEnumerable<Matricula>> ObterMatriculasPorAlunoIdAsync(Guid alunoId)
    {
        return await _context.Matriculas
             .AsNoTracking()
             .Where(m => m.AlunoId == alunoId)
             .ToListAsync();
    }

    public async Task AlterarStatusMatriculaAsync(Guid matriculaId, MatriculaStatus status)
    {
        var matricula = await _context.Matriculas.FindAsync(matriculaId);

        if (matricula != null)
        {
            matricula.AtualizarStatus(status);
            _context.Matriculas.Update(matricula);
        }
    }

    public async Task ConcluirAula(Guid matriculaId, Guid aulaId)
    {
        var matricula = await _context.Matriculas.FindAsync(matriculaId);
        if (matricula != null)
        {
            var aulaConcluida = new AulaConluida(matriculaId, aulaId);
            await _context.AulasConcluidas.AddAsync(aulaConcluida);
        }
    }

    public async Task<Matricula> ObterMatriculaPorId(Guid matriculaId)
    {
        return await _context.Matriculas.FindAsync(matriculaId);
    }

    public async Task<IEnumerable<AulaConluida>> ObterAulasConcluidasPorMatriculaId(Guid matriculaId)
    {
        return await _context.AulasConcluidas
            .AsNoTracking()
            .Where(ac => ac.MatriculaId == matriculaId)
            .ToListAsync();
    }

    public async Task<int> ContarAulasConcluidasPorMatriculaId(Guid matriculaId)
    {
        return await _context.AulasConcluidas
            .AsNoTracking()
            .Where(ac => ac.MatriculaId == matriculaId)
            .CountAsync();
    }

    public async Task<Guid?> AdicionarCertificadoAsync(Guid matriculaId)
    {
        var certificado = new Certificado(matriculaId);
        await _context.Certificados.AddAsync(certificado);
        return certificado.Id;
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async Task<Matricula?> ObterMatriculaPorAlunoIdCursoId(Guid alunoId, Guid cursoId)
    {
        var matricula = await _context.Matriculas
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.AlunoId == alunoId && m.CursoId == cursoId);
        return matricula;
    }
}