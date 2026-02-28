using TelesEducacao.Core.Messages;

namespace TelesEducacao.Conteudos.Application.Commands;

public class CriarAulaCommand : Command
{
	public string Titulo { get; set; }
	public string Conteudo { get; set; }
	public Guid CursoId { get; set; }

	public CriarAulaCommand(string titulo, string conteudo, Guid cursoId)
	{
		Titulo = titulo;
		Conteudo = conteudo;
		CursoId = cursoId;
	}

	public override bool EhValido()
	{
		throw new NotImplementedException();
	}
}
