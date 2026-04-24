using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using NetDevPack.Security.JwtSigningCredentials;
using NetDevPack.Security.JwtSigningCredentials.AspNetCore;
using System.Reflection;
using TelesEducacao.Auth.API.Configuration;
using TelesEducacao.Auth.Application.Extensions;
using TelesEducacao.Auth.Application.Services;
using TelesEducacao.Auth.Data;
using TelesEducacao.Auth.Data.Configuration;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;
using TelesEducacao.WebAPI.Core.Data;
using TelesEducacao.WebAPI.Core.Usuario;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddControllers();

// Database
builder.Services.AddDatabase<AuthDbContext>(builder.Configuration, builder.Environment);

// Identity
var appSettingsSection = builder.Configuration.GetSection("AppTokenSettings");
builder.Services.Configure<AppTokenSettings>(appSettingsSection);

builder.Services.AddJwksManager(options => options.Algorithm = Algorithm.ES256)
    .PersistKeysToDatabaseStore<AuthDbContext>();

builder.Services.AddMessageBusConfiguration(builder.Configuration);

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedEmail = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AuthDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

// Application Services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IAspNetUser, AspNetUser>();
builder.Services.AddScoped<IMediatorHandler, MediatorHandler>();
//Notifications
builder.Services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Teles Educação API de Autenticação",
        Version = "v1",
        Description = "Documentação da API de autenticação JWT",
    });
});

builder.Services.AddMemoryCache();

var app = builder.Build();

app.Services.UseDbMigrationAuthHelper();

// Swagger
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Teles Educação API Autenticação v1");
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseJwksDiscovery();

app.Run();
