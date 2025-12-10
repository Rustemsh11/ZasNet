using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.UseCases.Queries.Car.GetAllActiveCars;
using ZasNet.Application.UseCases.Queries.Car.GetAllCars;


namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
[Authorize]
public class CarController(IMediator mediator): ControllerBase
{
    [HttpGet]
    public async Task<List<CarDto>> GetAllCars([FromQuery] GetAllCarsRequest getAllCarsRequest, CancellationToken cancellationToken)
    {
        return await mediator.Send(getAllCarsRequest, cancellationToken);
    }
    
    [HttpGet]
    public async Task<List<CarDto>> GetActiveCars([FromQuery] GetAllActiveCarsRequest getAllActiveCars, CancellationToken cancellationToken)
    {
        return await mediator.Send(getAllActiveCars, cancellationToken);
    }
}
