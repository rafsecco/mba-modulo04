using Microsoft.Extensions.Options;
using TelesEducacao.Bff.Plataforma.Dtos;
using TelesEducacao.Bff.Plataforma.Extensions;

namespace TelesEducacao.Bff.Plataforma.Services;

public interface IAuthService
{
    Task<Guid> RegistrarAsync(RegistrarUsuarioDto dto, CancellationToken cancellationToken);

    Task<UsuarioRespostaLogin> LoginAsync(LoginUserDto dto, CancellationToken cancellationToken);

    Task<RefreshToken> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}

public class AuthService : Service, IAuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient, IOptions<AppServicesSettings> settings)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(settings.Value.AuthUrl);
    }

    public async Task<Guid> RegistrarAsync(RegistrarUsuarioDto dto, CancellationToken cancellationToken)
    {
        var conteudo = ObterConteudo(dto);
        var response = await _httpClient.PostAsync("/auth/registrar", conteudo, cancellationToken);

        if (!TratarErrosResponse(response)) return Guid.Empty;

        return await DeserializarObjetoResponse<Guid>(response);
    }

    public async Task<UsuarioRespostaLogin> LoginAsync(LoginUserDto dto, CancellationToken cancellationToken)
    {
        var conteudo = ObterConteudo(dto);
        var response = await _httpClient.PostAsync("/auth/acessar", conteudo, cancellationToken);

        if (!response.IsSuccessStatusCode) return null;

        return await DeserializarObjetoResponse<UsuarioRespostaLogin>(response);
    }

    public async Task<RefreshToken> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var conteudo = ObterConteudo(refreshToken);
        var response = await _httpClient.PostAsync("/auth/refresh-token", conteudo, cancellationToken);

        if (!response.IsSuccessStatusCode) return null;

        return await DeserializarObjetoResponse<RefreshToken>(response);
    }
}