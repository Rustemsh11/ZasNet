using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Commands.DispetcherEarning.SendDispetcherEarningReportToTelegram;

/// <summary>
/// Команда для отправки отчета по заработку диспетчера в Telegram
/// </summary>
public record SendDispetcherEarningReportToTelegramCommand(
    List<DispetcherEarningByFilterDto> Data) : IRequest;

