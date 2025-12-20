using MediatR;

namespace ZasNet.Application.UseCases.Commands.Services.UpdateService;

public record UpdateServiceCommand(
        int Id,
        string Name,
        decimal Price,
        int MeasureId,
        double MinVolume,
        decimal StandartPrecentForEmployee,
        decimal PrecentForMultipleEmployeers,
        decimal PrecentLaterOrderForEmployee,
        decimal PrecentLaterOrderForMultipleEmployeers
    ) : IRequest;

