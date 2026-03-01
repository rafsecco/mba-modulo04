using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;

namespace TelesEducacao.Conteudos.Application.Events;

public class CursoCriadoIntegrationEvent : IntegrationEvent
{
	public Guid Id { get; private set; }
	public string Nome { get; private set; }
	public string Descricao { get; private set; }
	public bool Ativo { get; private set; }
	public decimal Valor { get; private set; }
	public string ConteudoProgramaticoTitulo { get; private set; }
	public string ConteudoProgramaticoDescricao { get; private set; }

	public CursoCriadoIntegrationEvent(Guid id, string nome, string descricao, bool ativo, decimal valor, string conteudoProgramaticoTitulo, string conteudoProgramaticoDescricao)
	{
		AggregateId = id;
		Nome = nome;
		Descricao = descricao;
		Ativo = ativo;
		Valor = valor;
		ConteudoProgramaticoTitulo = conteudoProgramaticoTitulo;
		ConteudoProgramaticoDescricao = conteudoProgramaticoDescricao;
	}
}
