using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.UseCases.Commands.EmployeeEarning.SendEmployeeEarningReportToTelegram;
using ZasNet.Application.UseCases.Commands.EmployeeEarnings.EmployeeEarningUpdate;
using ZasNet.Application.UseCases.Queries.EmployeeEarning.EmployeeEarningReport;
using ZasNet.Application.UseCases.Queries.EmployeeEarning.GetEmployeeEarningByMounth;
using ZasNet.Application.UseCases.Queries.Orders.GetOrdersByFilter;

namespace ZasNet.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]/[action]")]
public class EmployeeEarningController(IMediator mediator): ControllerBase
{
    [HttpGet]
    public async Task<List<EmployeeEarningByFilterDto>> GetEmployeeEarningByMounth([FromQuery] GetEmployeeEarningByMounthRequest request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    [HttpPost]
    public async Task UpdateEmployeeEarning([FromBody] EmployeeEarningUpdateCommand request, CancellationToken cancellationToken)
    {
        await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Скачать отчет по заработку сотрудника в формате PDF
    /// </summary>
    /// <remarks>
    /// Принимает список данных о заработках сотрудника и формирует PDF отчет
    /// </remarks>
    [HttpPost]
    public async Task<FileContentResult> DownloadReport([FromBody] DownloadEmployeeEarningReportRequest request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Отправить отчет по заработку сотрудника в Telegram
    /// </summary>
    /// <remarks>
    /// Принимает список данных о заработках сотрудника, формирует PDF отчет и отправляет его в Telegram.
    /// У сотрудника должен быть привязан ChatId.
    /// </remarks>
    [HttpPost]
    public async Task SendReportToTelegram([FromBody] SendEmployeeEarningReportToTelegramCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
    }
}
