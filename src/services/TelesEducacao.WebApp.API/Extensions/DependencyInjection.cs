using MediatR;
using TelesEducacao.Alunos.Application.Commands;
using TelesEducacao.Alunos.Application.Queries;
using TelesEducacao.Alunos.Data.Repository;
using TelesEducacao.Alunos.Domain;
using TelesEducacao.Conteudos.Application.Services;
using TelesEducacao.Conteudos.Data;
using TelesEducacao.Conteudos.Data.Repository;
using TelesEducacao.Conteudos.Domain;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;
using TelesEducacao.Pagamentos.AntiCorruption;
using TelesEducacao.Pagamentos.Business;
using TelesEducacao.Pagamentos.Data;
using TelesEducacao.Pagamentos.Data.Repository;
using TelesEducacao.WebApp.API.AccessControl;

namespace TelesEducacao.WebApp.API.Extensions;

public static class DependencyInjection
{
    public static void RegisterServices(this IServiceCollection services)
    {
        // Mediator
        services.AddScoped<IMediatorHandler, MediatorHandler>();

        //Notifications
        services.AddScoped<INotificationHandler<DomainNotification>, DomainNotificationHandler>();

        // Conteudos
        services.AddScoped<ICursoRepository, CursoRepository>();
        services.AddScoped<ICursoAppService, CursoAppService>();
        services.AddScoped<ICargaHorariaService, CargaHorariaService>();
        services.AddScoped<ConteudosContext>();

        //Alunos
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAlunoRepository, AlunoRepository>();
        services.AddScoped<IAlunoQueries, AlunoQueries>();

        services.AddScoped<IRequestHandler<CriarAlunoCommand, bool>, CriarAlunoCommandHandler>();
        services.AddScoped<IRequestHandler<AdicionarMatriculaCommand, bool>, AdicionarMatriculaCommandHandler>();

        //Users
        services.AddScoped<IUserService, UserService>();

        // Pagamento
        services.AddScoped<IPagamentoRepository, PagamentoRepository>();
        services.AddScoped<IPagamentoService, PagamentoService>();
        services.AddScoped<IPagamentoCartaoCreditoFacade, PagamentoCartaoCreditoFacade>();
        services.AddScoped<IPayPalGateway, PayPalGateway>();
        services.AddScoped<Pagamentos.AntiCorruption.IConfigurationManager, Pagamentos.AntiCorruption.ConfigurationManager>();
        services.AddScoped<PagamentosContext>();

        services.AddScoped<IRequestHandler<CancelarMatriculaCommand, bool>, CancelarMatriculaCommandHandler>();
        services.AddScoped<IRequestHandler<AtivarMatriculaCommand, bool>, AtivarMatriculaCommandHandler>();
        services.AddScoped<IRequestHandler<ConluirAulaCommand, bool>, ConcluirAulaCommandHandler>();
        services.AddScoped<IRequestHandler<ConcluirCursoCommand, bool>, ConcluirCursoCommandHandler>();
    }
}