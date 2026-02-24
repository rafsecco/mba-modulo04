using TelesEducacao.Conteudo.API.Configurations;
using TelesEducacao.MessageBus;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiConfigurations(builder.Configuration);
builder.Services.AddMessageBus(builder.Configuration);
builder.Services.AddSwaggerConfigureServices();
builder.Services.RegisterServices();

var app = builder.Build();
app.UseDbMigrationHelper();
app.UseSwaggerConfiguration();
app.UseApiCoreConfigurations();

app.Run();
