using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using TelesEducacao.Alunos.Application.AutoMapper;
using TelesEducacao.Alunos.Application.Commands;
using TelesEducacao.Alunos.Application.Queries;
using TelesEducacao.Alunos.Data;
using TelesEducacao.Alunos.Data.Configuration;
using TelesEducacao.Alunos.Data.Repository;
using TelesEducacao.Alunos.Domain;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.WebAPI.Core.Identidade;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<AlunosContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAutoMapper(cfg => { },
    typeof(AlunosDtoToDomainMappingProfile),
    typeof(AlunosDomainToDtoMappingProfile));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

//Add services
builder.Services.AddScoped<IMediatorHandler, MediatorHandler>();
builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
builder.Services.AddScoped<IAlunoQueries, AlunoQueries>();
builder.Services.AddScoped<IRequestHandler<CriarAlunoCommand, bool>, CriarAlunoCommandHandler>();
builder.Services.AddScoped<IRequestHandler<AdicionarMatriculaCommand, bool>, AdicionarMatriculaCommandHandler>();

builder.Services.AddJwtConfiguration(builder.Configuration);

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Teles Educação API de Alunos",
        Version = "v1",
        Description = "Documentação da API de Alunos com autenticação JWT",
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
app.Services.UseDbMigrationAlunosHelper();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Teles Educação API Alunos v1");
});

app.UseHttpsRedirection();

app.UseAuthConfiguration();

app.MapControllers();

app.Run();