using AutoMapper;
using FluentValidation.Results;
using TelesEducacao.Conteudos.Application.Dtos;
using TelesEducacao.Conteudos.Domain;
using TelesEducacao.Core.Messages.CommomMessages.IntegrationEvents;

namespace TelesEducacao.Conteudos.Application.Services;

public class CursoAppService : ICursoAppService
{
    private readonly ICursoRepository _cursoRepository;
    private readonly IMapper _mapper;
    private readonly ICargaHorariaService _cargaHorariaService;

    public CursoAppService(ICursoRepository cursoRepository, IMapper mapper, ICargaHorariaService cargaHorariaService)
    {
        _cursoRepository = cursoRepository;
        _mapper = mapper;
        _cargaHorariaService = cargaHorariaService;
    }

	public async Task<ResponseMessage> Adicionar(CriaCursoDto criaCursoDto)
	{
		var validationResult = new ValidationResult();
		var curso = _mapper.Map<Curso>(criaCursoDto);
		_cursoRepository.Adicionar(curso);

		await _cursoRepository.UnitOfWork.Commit();

		

		return new ResponseMessage(validationResult);
	}

	public async Task<IEnumerable<CursoDto>> ObterTodos()
    {
        return _mapper.Map<IEnumerable<CursoDto>>(await _cursoRepository.ObterTodos());
    }

    public async Task<CursoDto> ObterPorId(Guid id)
    {
        return _mapper.Map<CursoDto>(await _cursoRepository.ObterPorId(id));
    }

    public async Task<IEnumerable<AulaDto>> ObterAulas(Guid cursoId)
    {
        return _mapper.Map<IEnumerable<AulaDto>>(await _cursoRepository.ObterAulas(cursoId));
    }

    public async Task<AulaDto> ObterAula(Guid aulaId)
    {
        return _mapper.Map<AulaDto>(await _cursoRepository.ObterAula(aulaId));
    }

    public async Task Atualizar(AtualizaCursoDto atualizaCursoDto)
    {
        var cursoDto = await ObterPorId(atualizaCursoDto.Id);
        if (cursoDto == null)
            throw new KeyNotFoundException("Curso não encontrado.");

        var curso = _mapper.Map<Curso>(cursoDto);

        if (!string.IsNullOrEmpty(atualizaCursoDto.Nome)) curso.AlterarNome(atualizaCursoDto.Nome);
        if (!string.IsNullOrEmpty(atualizaCursoDto.Descricao))
            curso.AlterarDescricao(atualizaCursoDto.Descricao);
        if (atualizaCursoDto.Valor.HasValue) curso.AlterarValor(atualizaCursoDto.Valor.Value);
        if (atualizaCursoDto.Ativo.HasValue && atualizaCursoDto.Ativo.Value is true) curso.Ativar();
        if (atualizaCursoDto.Ativo.HasValue && atualizaCursoDto.Ativo.Value is false) curso.Desativar();

        _cursoRepository.Atualizar(curso);

        await _cursoRepository.UnitOfWork.Commit();
    }

    public async Task<bool> Remover(Guid id)
    {
        var curso = await _cursoRepository.ObterPorId(id);
        _cursoRepository.Remover(curso);

        return await _cursoRepository.UnitOfWork.Commit();
    }

    public async Task<Guid?> AdicionarAula(CriaAulaDto criaAulaDto)
    {
        var aula = _mapper.Map<Aula>(criaAulaDto);
        _cursoRepository.AdicionarAula(aula);
        await _cursoRepository.UnitOfWork.Commit();
        return aula.Id;
    }

    public async Task<bool> RemoverAula(Guid aulaId)
    {
        await _cursoRepository.RemoverAula(aulaId);
        return await _cursoRepository.UnitOfWork.Commit();
    }

    public void Dispose()
    {
        _cursoRepository.Dispose();
    }
}
