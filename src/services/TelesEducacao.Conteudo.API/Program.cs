using TelesEducacao.Conteudo.API.Configurations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiConfigurations(builder.Configuration);
builder.Services.AddSwaggerConfigureServices();
builder.Services.RegisterServices();

var app = builder.Build();
app.UseDbMigrationHelper();
app.UseSwaggerConfiguration();
app.UseApiCoreConfigurations();

app.Run();
