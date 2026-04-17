using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TelesEducacao.Alunos.Domain;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Data;
using TelesEducacao.Core.Messages;

namespace TelesEducacao.Alunos.Data;

public class AlunosContext : DbContext, IUnitOfWork
{
    private readonly IMediatorHandler _mediatorHandler;

    public AlunosContext(DbContextOptions<AlunosContext> options, IMediatorHandler mediatorHandler) : base(options)
    {
        _mediatorHandler = mediatorHandler;
    }

    public DbSet<Aluno> Alunos { get; set; }
    public DbSet<Matricula> Matriculas { get; set; }
    public DbSet<AulaConluida> AulasConcluidas { get; set; }
    public DbSet<Certificado> Certificados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        //HACK: pra não setar string como varchar(max)
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(
            e => e.GetProperties().Where(p => p.ClrType == typeof(string))))
        {
            property.SetMaxLength(100);
        }

        modelBuilder.Ignore<Event>();

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public async Task<bool> Commit()
    {
        await _mediatorHandler.PublicarEventos(this);
        return await base.SaveChangesAsync() > 0;
    }
}