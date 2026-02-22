using Microsoft.OpenApi.Models;
using Polly;
using TelesEducacao.Bff.Plataforma.Extensions;
using TelesEducacao.Bff.Plataforma.Services;
using TelesEducacao.WebAPI.Core.Extensions;
using TelesEducacao.WebAPI.Core.Identidade;
using TelesEducacao.WebAPI.Core.Usuario;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.Configure<AppServicesSettings>(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddPolicy("Total",
        builder =>
            builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<IAspNetUser, AspNetUser>();

builder.Services.AddTransient<HttpClientAuthorizationDelegatingHandler>();

builder.Services.AddHttpClient<IAlunoService, AlunoService>()
    .AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
    .AllowSelfSignedCertificate()
    .AddPolicyHandler(PollyExtensions.EsperarTentar())
    .AddTransientHttpErrorPolicy(
        p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

builder.Services.AddJwtConfiguration(builder.Configuration);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Teles Educação BFF API Gateway",
        Version = "v1",
        Description = "BFF da plataforma Teles Educação",
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu token}",
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

app.UseCors("Total");
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Teles Educação BFF API Gateway v1");
});

app.UseHttpsRedirection();

app.UseAuthConfiguration();

app.MapControllers();

app.Run();