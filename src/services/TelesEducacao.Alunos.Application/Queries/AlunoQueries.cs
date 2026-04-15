using AutoMapper;
using TelesEducacao.Alunos.Application.Queries.Dtos;
using TelesEducacao.Alunos.Domain;

namespace TelesEducacao.Alunos.Application.Queries;

public class AlunoQueries : IAlunoQueries
{
    private readonly IMapper _mapper;
    private readonly IAlunoRepository _alunoRepository;

    public AlunoQueries(IAlunoRepository alunoRepository, IMapper mapper)
    {
        _alunoRepository = alunoRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        var alunos = await _alunoRepository.ObterTodosAsync(cancellationToken);
        return _mapper.Map<IEnumerable<AlunoDto>>(alunos);
    }

    public async Task<AlunoDto> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var aluno = await _alunoRepository.ObterPorIdAsync(id, cancellationToken);
        return _mapper.Map<AlunoDto>(aluno);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterMatriculasPorAlunoIdAsync(Guid alunoId, CancellationToken cancellationToken)
    {
        var matriculas = await _alunoRepository.ObterMatriculasPorAlunoIdAsync(alunoId, cancellationToken);
        return _mapper.Map<IEnumerable<MatriculaDto>>(matriculas);
    }

    public async Task<MatriculaDto> ObterMatriculaPorIdAsync(Guid matriculaId, CancellationToken cancellationToken)
    {
        var matricula = await _alunoRepository.ObterMatriculaPorIdAsync(matriculaId, cancellationToken);
        return _mapper.Map<MatriculaDto>(matricula);
    }

    public async Task<IEnumerable<AulaConcluidaDto>> ObterAulasConcluidasPorMatriculaIdAsync(Guid matriculaId, CancellationToken cancellationToken)
    {
        var aulasConcluidas = await _alunoRepository.ObterAulasConcluidasPorMatriculaIdAsync(matriculaId, cancellationToken);
        return _mapper.Map<IEnumerable<AulaConcluidaDto>>(aulasConcluidas);
    }

    public Task<MatriculaDto> ObterMatriculaPorAlunoIdCursoId(Guid alunoId, Guid cursoId)
    {
        var matricula = _alunoRepository.ObterMatriculaPorAlunoIdCursoId(alunoId, cursoId);
        return Task.FromResult(_mapper.Map<MatriculaDto>(matricula));
    }

    public async Task<int> ObterTotalAulasConcluidasPorMatriculaId(Guid matriculaId)
    {
        return await _alunoRepository.ContarAulasConcluidasPorMatriculaId(matriculaId);
    }
}