using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.UseCases.Commands.Cars.CreateCar;
using ZasNet.Application.UseCases.Commands.Cars.DeleteCar;
using ZasNet.Application.UseCases.Commands.Cars.UpdateCar;
using ZasNet.Application.UseCases.Queries.Car.GetAllActiveCars;
using ZasNet.Application.UseCases.Queries.Car.GetAllCars;
using ZasNet.Application.UseCases.Queries.Cars.GetCars;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
[Authorize]
public class CarController(IMediator mediator) : ControllerBase
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

    [HttpGet]
    public async Task<List<CarDto>> GetCars([FromQuery] GetCarsRequest getCarsRequest, CancellationToken cancellationToken)
    {
        return await mediator.Send(getCarsRequest, cancellationToken);
    }

    [HttpPost]
    public async Task CreateCar([FromBody] CreateCarRequest createCarRequest, CancellationToken cancellationToken) 
    {
        await mediator.Send(createCarRequest, cancellationToken);
    }
    
    [HttpPost]
    public async Task UpdateCar([FromBody] UpdateCarCommand updateCarCommand, CancellationToken cancellationToken) 
    {
        await mediator.Send(updateCarCommand, cancellationToken);
    }
    
    [HttpDelete]
    public async Task DeleteCar([FromBody] DeleteCarCommand deleteCarCommand, CancellationToken cancellationToken) 
    {
        await mediator.Send(deleteCarCommand, cancellationToken);
    }
}
