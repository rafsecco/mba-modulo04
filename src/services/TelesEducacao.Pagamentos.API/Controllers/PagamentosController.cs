using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TelesEducacao.Core.Communication.Mediator;
using TelesEducacao.Core.Messages.CommomMessages.Notifications;
using TelesEducacao.WebAPI.Core.Controllers;

namespace TelesEducacao.Pagamentos.API.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Roles = "Aluno")]
public class PagamentosController : MainController
{
    public PagamentosController(IMediatorHandler mediatorHandler, INotificationHandler<DomainNotification> notifications) : base(mediatorHandler, notifications)
    {
    }
}