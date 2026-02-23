using Microsoft.Extensions.Options;
using TelesEducacao.Bff.Plataforma.Dtos;
using TelesEducacao.Bff.Plataforma.Extensions;

namespace TelesEducacao.Bff.Plataforma.Services;

public interface IAlunoService
{
    Task<AlunoDto?> ObterPorId(Guid id, CancellationToken cancellationToken);
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
}