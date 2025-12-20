using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.UseCases.Commands.Services.CreateService;
using ZasNet.Application.UseCases.Commands.Services.DeleteService;
using ZasNet.Application.UseCases.Commands.Services.UpdateService;
using ZasNet.Application.UseCases.Queries.Services.GetServices;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
[Authorize]
public class ServiceController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<List<ServiceDto>> GetServices([FromQuery] GetServicesRequest getServicesRequest, CancellationToken cancellationToken)
    {
        return await mediator.Send(getServicesRequest, cancellationToken);
    }

    [HttpPost]
    public async Task CreateService([FromBody] CreateServiceRequest createServiceRequest, CancellationToken cancellationToken) 
    {
        await mediator.Send(createServiceRequest, cancellationToken);
    }
    
    [HttpPost]
    public async Task UpdateService([FromBody] UpdateServiceCommand updateServiceCommand, CancellationToken cancellationToken) 
    {
        await mediator.Send(updateServiceCommand, cancellationToken);
    }
    
    [HttpDelete]
    public async Task DeleteService([FromBody] DeleteServiceCommand deleteServiceCommand, CancellationToken cancellationToken) 
    {
        await mediator.Send(deleteServiceCommand, cancellationToken);
    }
}

