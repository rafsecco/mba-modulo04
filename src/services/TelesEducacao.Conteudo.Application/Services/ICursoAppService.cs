using TelesEducacao.Conteudos.Application.Dtos;
using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;

namespace TelesEducacao.Conteudos.Application.Services;

public interface ICursoAppService : IDisposable
{
    Task<IEnumerable<CursoDto>> ObterTodos();

    Task<CursoDto> ObterPorId(Guid id);

    Task<IEnumerable<AulaDto>> ObterAulas(Guid cursoId);

    Task<AulaDto> ObterAula(Guid aulaId);

    //Task<Guid?> Adicionar(CriaCursoDto criaCursoDto);
	Task<ResponseMessage> Adicionar(CriaCursoDto criaCursoDto);

	Task Atualizar(AtualizaCursoDto cursoDto);

    Task<bool> Remover(Guid id);

    Task<Guid?> AdicionarAula(CriaAulaDto criaAulaDto);

    Task<bool> RemoverAula(Guid id);
}
