using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.EmployeeEarning.EmployeeEarningReport;

/// <summary>
/// Запрос на скачивание отчета по заработку сотрудника
/// </summary>
public record DownloadEmployeeEarningReportRequest(
    List<EmployeeEarningByFilterDto> Data) : IRequest<FileContentResult>;

