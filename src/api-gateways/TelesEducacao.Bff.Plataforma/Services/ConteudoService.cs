using Microsoft.Extensions.Options;
using TelesEducacao.Bff.Plataforma.Extensions;
using TelesEducacao.Conteudos.Application.Dtos;

namespace TelesEducacao.Bff.Plataforma.Services;

public interface IConteudoService
{
	Task<List<CursoDto>> ObterTodos(CancellationToken cancellationToken);
	Task<CursoDto> ObterPorId(Guid id, CancellationToken cancellationToken);
	Task<List<AulaDto>> ObterAulasPorCurso(Guid cursoId, CancellationToken cancellationToken);
	Task<AulaDto> ObterAula(Guid cursoId, Guid aulaId, CancellationToken cancellationToken);
	Task<bool> Remove(Guid id, CancellationToken cancellationToken);

    Task<bool> Cria(CriaCursoDto criaCursoDto, CancellationToken cancellationToken);
	Task<bool> CriarAula(Guid cursoId, CriaAulaDto dto, CancellationToken cancellationToken);
	Task<bool> Atualiza(AtualizaCursoDto atualizaCursoDto, CancellationToken cancellationToken);
}

public class ConteudoService : Service, IConteudoService
{

	private readonly HttpClient _httpClient;

    public ConteudoService(HttpClient httpClient, IOptions<AppServicesSettings> settings)
	{
		_httpClient = httpClient;
		// use ConteudoUrl for the conteudo service
		_httpClient.BaseAddress = new Uri(settings.Value.ConteudoUrl);
	}

	public async Task<AulaDto> ObterAula(Guid cursoId, Guid aulaId, CancellationToken cancellationToken)
	{
		var response = await _httpClient.GetAsync($"/Conteudo/{cursoId}/aulas/{aulaId}", cancellationToken);

		if (!TratarErrosResponse(response))
		{
			return null;
		}

		return await DeserializarObjetoResponse<AulaDto>(response);
	}

	public async Task<List<AulaDto>> ObterAulasPorCurso(Guid cursoId, CancellationToken cancellationToken)
	{
		var response = await _httpClient.GetAsync($"/Conteudo/{cursoId}/aulas/", cancellationToken);

		if (!TratarErrosResponse(response))
		{
			return null;
		}

		return await DeserializarObjetoResponse<List<AulaDto>>(response);
	}

	public async Task<CursoDto> ObterPorId(Guid id, CancellationToken cancellationToken)
	{
		var response = await _httpClient.GetAsync($"/Conteudo/{id}", cancellationToken);

		if (!TratarErrosResponse(response))
		{
			return null;
		}

		return await DeserializarObjetoResponse<CursoDto>(response);
	}

	public async Task<List<CursoDto>> ObterTodos(CancellationToken cancellationToken)
	{
		var response = await _httpClient.GetAsync($"/Conteudo/", cancellationToken);

		if (!TratarErrosResponse(response))
		{
			return null;
		}

		return await DeserializarObjetoResponse<List<CursoDto>>(response);
	}

	public async Task<bool> Cria(CriaCursoDto criaCursoDto, CancellationToken cancellationToken)
	{
		var response = await _httpClient.PostAsJsonAsync("/Conteudo", criaCursoDto, cancellationToken);
		return TratarErrosResponse(response);
	}

	public async Task<bool> CriarAula(Guid cursoId, CriaAulaDto dto, CancellationToken cancellationToken)
	{
		dto.CursoId = cursoId;
		var response = await _httpClient.PostAsJsonAsync($"/Conteudo/{cursoId}/aulas", dto, cancellationToken);
		return TratarErrosResponse(response);
	}

	public async Task<bool> Atualiza(AtualizaCursoDto atualizaCursoDto, CancellationToken cancellationToken)
	{
		var response = await _httpClient.PutAsJsonAsync("/Conteudo", atualizaCursoDto, cancellationToken);
		return TratarErrosResponse(response);
	}

	public async Task<bool> Remove(Guid id, CancellationToken cancellationToken)
	{
 	var response = await _httpClient.DeleteAsync($"/Conteudo/{id}", cancellationToken);
		return TratarErrosResponse(response);
	}
}
