using Microsoft.AspNetCore.Mvc;
using MediatR;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;

namespace TelesEducacao.Auth.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    private readonly IMediatorHandler _mediatorHandler;
    private readonly DomainNotificationHandler _notifications;

    protected BaseController(IMediatorHandler mediatorHandler,
        INotificationHandler<DomainNotification> notifications)
    {
        _mediatorHandler = mediatorHandler;
        _notifications = (DomainNotificationHandler)notifications;
    }

    protected bool OperacaoValida()
    {
        return !_notifications.TemNotificacoes();
    }

    protected async Task NotificarErro(string codigo, string mensagem)
    {
        await _mediatorHandler.PublicarNotificacao(new DomainNotification(codigo, mensagem));
    }

    protected string ObterMensagemErro()
    {
        var notificacoes = _notifications.ObterNotificacoes()
            .Select(c => c.Value);

        return string.Join(" | ", notificacoes);
    }

    protected new IActionResult Response(object? result = null)
    {
        if (OperacaoValida())
        {
            return Ok(new
            {
                success = true,
                data = result
            });
        }

        return BadRequest(new
        {
            success = false,
            errors = _notifications.ObterNotificacoes().Select(n => n.Value)
        });
    }
}