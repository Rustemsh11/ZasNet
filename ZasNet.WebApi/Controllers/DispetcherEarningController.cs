using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.UseCases.Commands.DispetcherEarning.SendDispetcherEarningReportToTelegram;
using ZasNet.Application.UseCases.Commands.DispetcherEarnings.DispetcherEarningUpdate;
using ZasNet.Application.UseCases.Queries.DispetcherEarnings.DispetcherEarningReport;
using ZasNet.Application.UseCases.Queries.DispetcherEarnings.GetDispetcherEarningByMounth;

namespace ZasNet.WebApi.Controllers;

[ApiController]
[Route("api/v1/[controller]/[action]")]
public class DispetcherEarningController(IMediator mediator): ControllerBase
{
    [HttpGet]
    public async Task<List<DispetcherEarningByFilterDto>> GetDispetcherEarningByMounth([FromQuery] GetDispetcherEarningByMounthRequest request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    [HttpPost]
    public async Task UpdateDispetcherEarning([FromBody] DispetcherEarningUpdateCommand request, CancellationToken cancellationToken)
    {
        await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Скачать отчет по заработку диспетчера в формате PDF
    /// </summary>
    /// <remarks>
    /// Принимает список данных о заработках диспетчера и формирует PDF отчет
    /// </remarks>
    [HttpPost]
    public async Task<FileContentResult> DownloadReport([FromBody] DownloadDispetcherEarningReportRequest request, CancellationToken cancellationToken)
    {
        return await mediator.Send(request, cancellationToken);
    }

    /// <summary>
    /// Отправить отчет по заработку диспетчера в Telegram
    /// </summary>
    /// <remarks>
    /// Принимает список данных о заработках диспетчера, формирует PDF отчет и отправляет его в Telegram.
    /// У диспетчера должен быть привязан ChatId.
    /// </remarks>
    [HttpPost]
    public async Task SendReportToTelegram([FromBody] SendDispetcherEarningReportToTelegramCommand command, CancellationToken cancellationToken)
    {
        await mediator.Send(command, cancellationToken);
    }
}

