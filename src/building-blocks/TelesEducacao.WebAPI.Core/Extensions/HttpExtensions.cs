using Microsoft.Extensions.DependencyInjection;

namespace TelesEducacao.WebAPI.Core.Extensions;

public static class HttpExtensions
{
    public static IHttpClientBuilder AllowSelfSignedCertificate(this IHttpClientBuilder builder)
    {
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        return builder.ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
        });
    }
}