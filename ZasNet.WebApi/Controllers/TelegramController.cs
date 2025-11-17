using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using ZasNet.Application.UseCases.Commands.Orders.ChangeOrderStatus;

namespace ZasNet.WebApi.Controllers;

[ApiController]
[Route("telegram")]
public class TelegramController(IMediator mediator) : ControllerBase
{
    private readonly IMediator _mediator = mediator;

    [HttpPost]
    public Task Post(Update update)
    {
        Console.WriteLine(update.Message.Text);
        return Task.CompletedTask;
    }

    [HttpPost("update/{secret}")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveUpdate([FromRoute] string secret, [FromBody] Update update, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ChangeOrderStausFromTgCommand(secret, update), cancellationToken);
        return StatusCode(result.StatusCode);
    }
}

