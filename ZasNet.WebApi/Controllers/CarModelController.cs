using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.UseCases.Commands.CarModels.CreateCarModel;
using ZasNet.Application.UseCases.Commands.CarModels.DeleteCarModel;
using ZasNet.Application.UseCases.Commands.CarModels.UpdateCarModel;
using ZasNet.Application.UseCases.Queries.CarModels.GetCarModels;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
[Authorize]
public class CarModelController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<List<CarModelDto>> GetCarModels([FromQuery] GetCarModelsRequest getCarModelsRequest, CancellationToken cancellationToken)
    {
        return await mediator.Send(getCarModelsRequest, cancellationToken);
    }

    [HttpPost]
    public async Task CreateCarModel([FromBody] CreateCarModelRequest createCarModelRequest, CancellationToken cancellationToken) 
    {
        await mediator.Send(createCarModelRequest, cancellationToken);
    }
    
    [HttpPost]
    public async Task UpdateCarModel([FromBody] UpdateCarModelCommand updateCarModelCommand, CancellationToken cancellationToken) 
    {
        await mediator.Send(updateCarModelCommand, cancellationToken);
    }
    
    [HttpDelete]
    public async Task DeleteCarModel([FromBody] DeleteCarModelCommand deleteCarModelCommand, CancellationToken cancellationToken) 
    {
        await mediator.Send(deleteCarModelCommand, cancellationToken);
    }
}

