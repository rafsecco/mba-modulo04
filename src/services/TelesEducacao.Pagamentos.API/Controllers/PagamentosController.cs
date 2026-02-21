using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.DomainObjects;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;
using TelesEducacao.Pagamentos.Business;
using TelesEducacao.Pagamentos.Data;

using ControllerBase = TelesEducacao.WebAPI.Core.Controllers.ControllerBase;

namespace TelesEducacao.Pagamentos.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Aluno")]
public class PagamentosController : ControllerBase
{
    private readonly IPagamentoService _pagamentoService;

    public PagamentosController(
        INotificationHandler<DomainNotification> notifications,
        IMediatorHandler mediatorHandler,
        IPagamentoService pagamentoService
    ) : base(mediatorHandler, notifications)
    {
        _pagamentoService = pagamentoService;
    }

    // DTO de entrada da API
    public class RealizarPagamentoMatriculaRequest
    {
        public Guid MatriculaId { get; set; }
        public Guid AlunoId { get; set; }
        public decimal Valor { get; set; }

        public string NomeCartao { get; set; } = string.Empty;
        public string NumeroCartao { get; set; } = string.Empty;
        public string ExpiracaoCartao { get; set; } = string.Empty;
        public string CvvCartao { get; set; } = string.Empty;
    }

    public class RealizarPagamentoMatriculaResponse
    {
        public Guid PagamentoId { get; set; }
        public Guid TransacaoId { get; set; }
        public StatusTransacao Status { get; set; }
    }

   
    [HttpPost("Matriculas/{matriculaId:guid}")]
    [ProducesResponseType(typeof(RealizarPagamentoMatriculaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RealizarPagamentoMatriculaResponse>> RealizarPagamentoMatricula(
        Guid matriculaId,
        [FromBody] RealizarPagamentoMatriculaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            
            if (matriculaId == Guid.Empty)
                return BadRequest(new { message = "matriculaId inválido." });

            if (request is null)
                return BadRequest(new { message = "Request inválido." });

            if (request.MatriculaId == Guid.Empty)
                return BadRequest(new { message = "MatriculaId é obrigatório." });

            if (request.AlunoId == Guid.Empty)
                return BadRequest(new { message = "AlunoId é obrigatório." });

            if (request.Valor <= 0)
                return BadRequest(new { message = "Valor deve ser maior que zero." });

            
            if (request.MatriculaId != matriculaId)
                return BadRequest(new { message = "MatriculaId do body deve ser igual ao da rota." });

            var pagamentoMatricula = new PagamentoMatricula
            {
                MatriculaId = request.MatriculaId,
                AlunoId = request.AlunoId,
                Valor = request.Valor,
                NomeCartao = request.NomeCartao,
                NumeroCartao = request.NumeroCartao,
                ExpiracaoCartao = request.ExpiracaoCartao,
                CvvCartao = request.CvvCartao
            };

            var transacao = await _pagamentoService.RealizarPagamentoMatricula(pagamentoMatricula);

           
            return StatusCode(StatusCodes.Status201Created, new RealizarPagamentoMatriculaResponse
            {
                PagamentoId = transacao.PagamentoId,
                TransacaoId = transacao.Id,
                Status = transacao.StatusTransacao
            });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    public class StatusPagamentoResponse
    {
        public Guid MatriculaId { get; set; }
        public Guid PagamentoId { get; set; }
        public Guid TransacaoId { get; set; }
        public StatusTransacao Status { get; set; }
        public decimal Total { get; set; }
        public DateTime DataCadastro { get; set; }
    }
    
    [AllowAnonymous] 
    [HttpGet("Matriculas/{matriculaId:guid}/Status")]
    [ProducesResponseType(typeof(StatusPagamentoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StatusPagamentoResponse>> ConsultarStatusPorMatricula(
        Guid matriculaId,
        [FromServices] PagamentosContext context,
        CancellationToken cancellationToken)
    {
        if (matriculaId == Guid.Empty)
            return BadRequest(new { message = "matriculaId inválido." });

        var transacao = await context.Transacoes
            .AsNoTracking()
            .Where(t => t.MatriculaId == matriculaId)
            .OrderByDescending(t => t.DataCadastro)
            .FirstOrDefaultAsync(cancellationToken);

        if (transacao is null)
            return NotFound(new { message = "Nenhuma transação encontrada para esta matrícula." });

        return Ok(new StatusPagamentoResponse
        {
            MatriculaId = transacao.MatriculaId,
            PagamentoId = transacao.PagamentoId,
            TransacaoId = transacao.Id,
            Status = transacao.StatusTransacao,
            Total = transacao.Total,
            DataCadastro = transacao.DataCadastro
        });
    }
}