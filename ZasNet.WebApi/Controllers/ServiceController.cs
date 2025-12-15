using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;
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
}

