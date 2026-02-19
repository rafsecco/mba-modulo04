using MediatR;
using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;

namespace TelesEducacao.WebAPI.Core.Controllers;

public class ControllerBase : Controller
{
    private readonly IMediatorHandler _mediatorHandler;
    private readonly DomainNotificationHandler _notifications;

    public ControllerBase(IMediatorHandler mediatorHandler,
        INotificationHandler<DomainNotification> notifications)
    {
        _mediatorHandler = mediatorHandler;
        _notifications = (DomainNotificationHandler)notifications;
    }

    protected bool OperacaoValida()
    {
        return !_notifications.TemNotificacoes();
    }

    protected void NotificarErro(string codigo, string mensagem)
    {
        _mediatorHandler.PublicarNotificacao(new DomainNotification(codigo, mensagem));
    }

    protected string ObterMensagemErro()
    {
        var notificacoes = _notifications.ObterNotificacoes();
        var mensagem = string.Join(Environment.NewLine, notificacoes.Select(n => n.Value));
        return mensagem;
    }
}