using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;

namespace TelesEducacao.Conteudos.Application.Events;

public class AulaCriadaIntegrationEvent : IntegrationEvent
{
	public string Titulo { get; set; }
	public string Conteudo { get; set; }
	public Guid CursoId { get; set; }

	public AulaCriadaIntegrationEvent(string titulo, string conteudo, Guid cursoId)
	{
		Titulo = titulo;
		Conteudo = conteudo;
		CursoId = cursoId;
	}
}
