using System.ComponentModel.DataAnnotations;

namespace TelesEducacao.Alunos.API.Dtos;

public class AdicionarMatriculaDto
{
    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    public Guid AlunoId { get; init; }

    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    public Guid CursoId { get; init; }

    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [StringLength(250, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 5)]
    public string NomeCartao { get; init; }

    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [StringLength(16, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 16)]
    public string NumeroCartao { get; init; }

    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [StringLength(10, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 6)]
    public string ExpiracaoCartao { get; init; }

    [Required(ErrorMessage = "O campo {0} é obrigatório")]
    [StringLength(4, ErrorMessage = "O campo {0} precisa ter entre {2} e {1} caracteres", MinimumLength = 3)]
    public string CvvCartao { get; init; }
}