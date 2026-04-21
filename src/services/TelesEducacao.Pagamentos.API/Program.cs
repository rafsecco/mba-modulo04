using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Pagamentos.AntiCorruption;
using TelesEducacao.Pagamentos.API.Configuration;
using TelesEducacao.Pagamentos.API.Controllers;
using TelesEducacao.Pagamentos.API.Models;
using TelesEducacao.Pagamentos.Business;
using TelesEducacao.Pagamentos.Data;
using TelesEducacao.Pagamentos.Data.Repository;
using TelesEducacao.WebAPI.Core.Database;
using TelesEducacao.WebAPI.Core.Identidade;
using TelesEducacao.Pagamentos.Data.Configuration;


// aliases pra evitar conflito com IConfigurationManager do .NET
using IPagamentosConfigManager = TelesEducacao.Pagamentos.AntiCorruption.IConfigurationManager;
using PagamentosConfigManager = TelesEducacao.Pagamentos.AntiCorruption.ConfigurationManager;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDatabase<PagamentosContext>(builder.Configuration, builder.Environment);

builder.Services.AddScoped<IPagamentoService, PagamentoService>();
builder.Services.AddScoped<IPagamentoCartaoCreditoFacade, PagamentoCartaoCreditoFacade>();
builder.Services.AddScoped<IPayPalGateway, PayPalGateway>();
builder.Services.AddSingleton<IPagamentosConfigManager, PagamentosConfigManager>();

builder.Services.AddScoped<IPagamentoRepository, PagamentoRepository>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(
        typeof(PagamentosController).Assembly, // API
        typeof(PagamentoService).Assembly,     // Business
        typeof(PagamentosContext).Assembly     // Data
    ));

builder.Services.AddScoped<IMediatorHandler, MediatorHandler>();


builder.Services.AddJwtConfiguration(builder.Configuration);

builder.Services.AddMessageBusConfiguration(builder.Configuration);

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Teles Educação API de Pagamentos",
        Version = "v1",
        Description = "Documentação da API de Pagamentos com autenticação JWT",
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.Services.UseDbMigrationPagamentosHelper();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Teles Educação API Pagamentos v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthConfiguration();

app.MapControllers();

app.Run();
