using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.DispetcherEarnings.DispetcherEarningReport;

/// <summary>
/// Запрос на скачивание отчета по заработку диспетчера
/// </summary>
public record DownloadDispetcherEarningReportRequest(
    List<DispetcherEarningByFilterDto> Data) : IRequest<FileContentResult>;
