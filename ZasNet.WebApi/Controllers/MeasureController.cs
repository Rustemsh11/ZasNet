using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.UseCases.Commands.Measures.CreateMeasure;
using ZasNet.Application.UseCases.Commands.Measures.DeleteMeasure;
using ZasNet.Application.UseCases.Commands.Measures.UpdateMeasure;
using ZasNet.Application.UseCases.Queries.Measures.GetMeasures;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
[Authorize]
public class MeasureController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<List<MeasureDto>> GetMeasures([FromQuery] GetMeasuresRequest getMeasuresRequest, CancellationToken cancellationToken)
    {
        return await mediator.Send(getMeasuresRequest, cancellationToken);
    }

    [HttpPost]
    public async Task CreateMeasure([FromBody] CreateMeasureRequest createMeasureRequest, CancellationToken cancellationToken) 
    {
        await mediator.Send(createMeasureRequest, cancellationToken);
    }
    
    [HttpPost]
    public async Task UpdateMeasure([FromBody] UpdateMeasureCommand updateMeasureCommand, CancellationToken cancellationToken) 
    {
        await mediator.Send(updateMeasureCommand, cancellationToken);
    }
    
    [HttpDelete]
    public async Task DeleteMeasure([FromBody] DeleteMeasureCommand deleteMeasureCommand, CancellationToken cancellationToken) 
    {
        await mediator.Send(deleteMeasureCommand, cancellationToken);
    }
}

