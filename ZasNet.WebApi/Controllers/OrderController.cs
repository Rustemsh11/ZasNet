using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.UseCases.Commands.Orders.CreateOrder;
using ZasNet.Application.UseCases.Queries.Orders.GetOrders;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
[Authorize]
public class OrderController(IMediator mediator): ControllerBase
{
    [HttpGet]
    public async Task GetOrders([FromQuery]GetOrdersRequest getOrdersRequest, CancellationToken cancellationToken)
    {
        await mediator.Send(getOrdersRequest, cancellationToken);
    }
    
    [HttpPost]
    public async Task CreateOrderCommand([FromBody] CreateOrderCommand createOrderCommand, CancellationToken token)
    {
        await mediator.Send(createOrderCommand, token);
    }
}
