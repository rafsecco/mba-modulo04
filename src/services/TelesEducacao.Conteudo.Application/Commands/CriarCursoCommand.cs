using TelesEducacao.Core.Messages;

namespace TelesEducacao.Conteudos.Application.Commands;

public class CriarCursoCommand : Command
{
	public string Nome { get; set; }
	public string Descricao { get; set; }
	public bool Ativo { get; set; }
	public decimal Valor { get; set; }
	public string ConteudoProgramaticoTitulo { get; set; }
	public string ConteudoProgramaticoDescricao { get; set; }

	public CriarCursoCommand(string nome, string descricao, bool ativo, decimal valor, string conteudoProgramaticoTitulo, string conteudoProgramaticoDescricao)
	{
		Nome = nome;
		Descricao = descricao;
		Ativo = ativo;
		Valor = valor;
		ConteudoProgramaticoTitulo = conteudoProgramaticoTitulo;
		ConteudoProgramaticoDescricao = conteudoProgramaticoDescricao;
	}

	public override bool EhValido()
	{
		throw new NotImplementedException();
	}
}
