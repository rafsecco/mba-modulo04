using System.ComponentModel.DataAnnotations;

namespace TelesEducacao.Bff.Plataforma.Dtos;

public class AulaDto
{
	[Key]
	public Guid Id { get; set; }

	[Required(ErrorMessage = "O campo {0} é obrigatório")]
	public Guid CursoId { get; set; }

	[Required(ErrorMessage = "O campo {0} é obrigatório")]
	public string Titulo { get; set; }

	[Required(ErrorMessage = "O campo {0} é obrigatório")]
	public string Conteudo { get; set; }

	[Required(ErrorMessage = "O campo {0} é obrigatório")]
	public TimeSpan Duracao { get; set; }
}

public class CriaAulaDto
{
	[Required(ErrorMessage = "O campo {0} é obrigatório")]
	public Guid CursoId { get; set; }

	[Required(ErrorMessage = "O campo {0} é obrigatório")]
	public string Titulo { get; set; }

	[Required(ErrorMessage = "O campo {0} é obrigatório")]
	public string Conteudo { get; set; }
}
