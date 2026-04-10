using AutoMapper;
using TelesEducacao.Alunos.Application.Queries.Dtos;
using TelesEducacao.Alunos.Domain;

namespace TelesEducacao.Alunos.Application.AutoMapper;

public class AlunosDomainToDtoMappingProfile : Profile
{
    public AlunosDomainToDtoMappingProfile()
    {
        //CreateMap<Source, Destination>();
        CreateMap<Aluno, AlunoDto>();
        CreateMap<Matricula, MatriculaDto>();
        CreateMap<AulaConluida, AulaConcluidaDto>();
    }
}