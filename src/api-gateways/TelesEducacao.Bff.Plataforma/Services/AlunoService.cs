using Microsoft.Extensions.Options;
using TelesEducacao.Bff.Plataforma.Dtos;
using TelesEducacao.Bff.Plataforma.Extensions;

namespace TelesEducacao.Bff.Plataforma.Services;

public interface IAlunoService
{
    Task<AlunoDto?> ObterPorId(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<AlunoDto>> ObterTodos(CancellationToken cancellationToken);

    Task<IEnumerable<MatriculaDto>> ObterMatriculasPorAlunoId(Guid id, CancellationToken cancellationToken);

    Task<bool> ConcluirAula(Guid id, Guid aulaId, CancellationToken cancellationToken);

    Task<bool> SolicitarFinalizacaoCurso(Guid matriculaId, CancellationToken cancellationToken);

    Task<bool> AdicionarMatricula(Guid id, Guid cursoId, AdicionarMatriculaDto matriculaDto, CancellationToken cancellationToken);
}

public class AlunoService : Service, IAlunoService
{
    private readonly HttpClient _httpClient;

    public AlunoService(HttpClient httpClient, IOptions<AppServicesSettings> settings)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(settings.Value.AlunoUrl);
    }

    public async Task<AlunoDto?> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/alunos/{id}", cancellationToken);

        if (!TratarErrosResponse(response))
        {
            return null;
        }

        return await DeserializarObjetoResponse<AlunoDto>(response);
    }

    public async Task<IEnumerable<AlunoDto>> ObterTodos(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("/alunos/", cancellationToken);
        if (!TratarErrosResponse(response))
        {
            return [];
        }

        return await DeserializarObjetoResponse<IEnumerable<AlunoDto>>(response);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterMatriculasPorAlunoId(Guid id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/alunos/{id}/Matriculas", cancellationToken);
        if (!TratarErrosResponse(response))
        {
            return [];
        }

        return await DeserializarObjetoResponse<IEnumerable<MatriculaDto>>(response);
    }

    public async Task<bool> ConcluirAula(Guid id, Guid aulaId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync($"/alunos/{id}/Matricula/{aulaId}/AulasConcluidas", null, cancellationToken);
        return TratarErrosResponse(response);
    }

    public async Task<bool> SolicitarFinalizacaoCurso(Guid matriculaId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync($"/alunos/{matriculaId}/matricula/certificados", null, cancellationToken);
        return TratarErrosResponse(response);
    }

    public async Task<bool> AdicionarMatricula(Guid id, Guid cursoId, AdicionarMatriculaDto matriculaDto, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync($"/alunos/{id}/Matricula/{cursoId}", matriculaDto, cancellationToken);
        return TratarErrosResponse(response);
    }
}