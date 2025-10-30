using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.UseCases.Commands.Orders.CreateOrder;
using ZasNet.Application.UseCases.Queries.Orders.GetOrders;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
public class OrderController(IMediator mediator): ControllerBase
{
    [HttpGet]
    [Authorize]
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
