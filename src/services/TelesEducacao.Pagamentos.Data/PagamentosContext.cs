using Microsoft.EntityFrameworkCore;
using System.Reflection;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Data;
using TelesEducacao.Core.Messages;
using TelesEducacao.Pagamentos.Business;

namespace TelesEducacao.Pagamentos.Data;

public class PagamentosContext : DbContext, IUnitOfWork
{
    private readonly IMediatorHandler _mediatorHandler;

    public PagamentosContext(DbContextOptions<PagamentosContext> options, IMediatorHandler rebusHandler)
        : base(options)
    {
        _mediatorHandler = rebusHandler ?? throw new ArgumentNullException(nameof(rebusHandler));
    }

    public DbSet<Pagamento> Pagamentos { get; set; }
    public DbSet<Transacao> Transacoes { get; set; }

    public async Task<bool> Commit()
    {
        var sucesso = await base.SaveChangesAsync() > 0;
        if (sucesso) await _mediatorHandler.PublicarEventos(this);

        return sucesso;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
		if (Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
		{
			//HACK: pra não setar string como varchar(max)
			foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(
				e => e.GetProperties().Where(p => p.ClrType == typeof(string))))
			{
				property.SetMaxLength(100);
			}
		}

		modelBuilder.Ignore<Event>();

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys())) relationship.DeleteBehavior = DeleteBehavior.ClientSetNull;
        base.OnModelCreating(modelBuilder);
    }
}
