using Microsoft.Extensions.Options;
using TelesEducacao.Bff.Plataforma.Dtos;
using TelesEducacao.Bff.Plataforma.Extensions;

namespace TelesEducacao.Bff.Plataforma.Services;

public interface IAlunoService
{
    Task<AlunoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken cancellationToken);

    Task<IEnumerable<MatriculaDto>> ObterMatriculasPorAlunoIdAsync(Guid id, CancellationToken cancellationToken);

    Task<MatriculaDto?> ObterMatriculaPorIdAsync(Guid matriculaId, CancellationToken cancellationToken);

    Task<IEnumerable<AulaConcluidaDto>> ObterAulasConcluidasPorMatriculaIdAsync(Guid matriculaId, CancellationToken cancellationToken);

    Task<bool> ConcluirAulaAsync(Guid matriculaId, Guid aulaId, CancellationToken cancellationToken);

    Task<bool> SolicitarConclusaoCursoAsync(Guid matriculaId, int totalAulasCurso, CancellationToken cancellationToken);

    Task<bool> AdicionarMatriculaAsync(Guid id, AdicionarMatriculaDto matriculaDto, CancellationToken cancellationToken);
}

public class AlunoService : Service, IAlunoService
{
    private readonly HttpClient _httpClient;

    public AlunoService(HttpClient httpClient, IOptions<AppServicesSettings> settings)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(settings.Value.AlunoUrl);
    }

    public async Task<AlunoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/alunos/{id}", cancellationToken);

        if (!TratarErrosResponse(response))
        {
            return null;
        }

        return await DeserializarObjetoResponse<AlunoDto>(response);
    }

    public async Task<IEnumerable<AlunoDto>> ObterTodosAsync(CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync("/alunos/", cancellationToken);
        if (!TratarErrosResponse(response))
        {
            return [];
        }

        return await DeserializarObjetoResponse<IEnumerable<AlunoDto>>(response);
    }

    public async Task<IEnumerable<MatriculaDto>> ObterMatriculasPorAlunoIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/alunos/{id}/matriculas", cancellationToken);
        if (!TratarErrosResponse(response))
        {
            return [];
        }

        return await DeserializarObjetoResponse<IEnumerable<MatriculaDto>>(response);
    }

    public async Task<bool> ConcluirAulaAsync(Guid matriculaId, Guid aulaId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsync($"/alunos/matriculas/{matriculaId}/aulas/{aulaId}/concluir", null, cancellationToken);
        return TratarErrosResponse(response);
    }

    public async Task<bool> SolicitarConclusaoCursoAsync(Guid matriculaId, int totalAulasCurso, CancellationToken cancellationToken)
    {
        var conteudo = ObterConteudo(totalAulasCurso);
        var response = await _httpClient.PostAsync($"/alunos/matriculas/{matriculaId}/concluir", conteudo, cancellationToken);
        return TratarErrosResponse(response);
    }

    public async Task<bool> AdicionarMatriculaAsync(Guid id, AdicionarMatriculaDto matriculaDto, CancellationToken cancellationToken)
    {
        var conteudo = ObterConteudo(matriculaDto);
        var response = await _httpClient.PostAsJsonAsync($"/alunos/{id}/matriculas", conteudo, cancellationToken);
        return TratarErrosResponse(response);
    }

    public async Task<MatriculaDto?> ObterMatriculaPorIdAsync(Guid matriculaId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/alunos/matriculas/{matriculaId}", cancellationToken);
        if (!TratarErrosResponse(response))
        {
            return null;
        }

        return await DeserializarObjetoResponse<MatriculaDto>(response);
    }

    public async Task<IEnumerable<AulaConcluidaDto>> ObterAulasConcluidasPorMatriculaIdAsync(Guid matriculaId, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync($"/alunos/matriculas/{matriculaId}/aulas-concluidas", cancellationToken);
        if (!TratarErrosResponse(response))
        {
            return [];
        }

        return await DeserializarObjetoResponse<IEnumerable<AulaConcluidaDto>>(response);
    }
}