using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.UseCases.Commands.Orders.ChangeOrderStatusToWaitingInvoice;
using ZasNet.Application.UseCases.Commands.Orders.CreateOrder;
using ZasNet.Application.UseCases.Commands.Orders.SaveOrder;
using ZasNet.Application.UseCases.Queries.Orders.GetCreateOrderParameters;
using ZasNet.Application.UseCases.Queries.Orders.GetOrder;
using ZasNet.Application.UseCases.Queries.Orders.GetOrders;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
[Authorize]
public class OrderController(IMediator mediator): ControllerBase
{
    [HttpGet]
    public async Task<List<GetOrdersResponse>> GetOrders([FromQuery]GetOrdersRequest getOrdersRequest, CancellationToken cancellationToken)
    {
        return await mediator.Send(getOrdersRequest, cancellationToken);
    }
    
    [HttpGet]
    public async Task<OrderDto> GetOrder([FromQuery]GetOrderRequest getOrderRequest, CancellationToken cancellationToken)
    {
        return await mediator.Send(getOrderRequest, cancellationToken);
    }
    
    [HttpGet]
    public async Task<CreateOrderParameters> GetCreateOrderParametSers([FromQuery]GetCreateOrderParametersRequest getCreateOrderParameters, CancellationToken cancellationToken)
    {
        return await mediator.Send(getCreateOrderParameters, cancellationToken);
    }
    
    [HttpPost]
    public async Task CreateOrder([FromBody] CreateOrderCommand createOrderCommand, CancellationToken token)
    {
        await mediator.Send(createOrderCommand, token);
    }
    
    [HttpPost]
    public async Task SaveOrder([FromBody] SaveOrderCommand saveOrderCommand, CancellationToken token)
    {
        await mediator.Send(saveOrderCommand, token);
    }
    
    [HttpPost]
    public async Task ChangeStatusToWaitingInvoice([FromBody] ChangeOrderStatusToWaitingInvoiceCommand command, CancellationToken token)
    {
        await mediator.Send(command, token);
    }
}
