using TelesEducacao.Conteudo.API.Configurations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiConfigurations(builder.Configuration);
builder.Services.AddSwaggerConfigureServices();

var app = builder.Build();
app.UseSwaggerConfiguration();
app.UseApiCoreConfigurations();

app.Run();
