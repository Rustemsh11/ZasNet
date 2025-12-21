using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Commands.EmployeeEarning.SendEmployeeEarningReportToTelegram;

/// <summary>
/// Команда для отправки отчета по заработку сотрудника в Telegram
/// </summary>
public record SendEmployeeEarningReportToTelegramCommand(
    List<EmployeeEarningByFilterDto> Data) : IRequest;

