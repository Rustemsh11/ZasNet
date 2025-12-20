using MediatR;

namespace ZasNet.Application.UseCases.Commands.Services.CreateService;

public record CreateServiceRequest(
        string Name,
        decimal Price,
        int MeasureId,
        double MinVolume,
        decimal StandartPrecentForEmployee,
        decimal PrecentForMultipleEmployeers,
        decimal PrecentLaterOrderForEmployee,
        decimal PrecentLaterOrderForMultipleEmployeers
    ) : IRequest;

