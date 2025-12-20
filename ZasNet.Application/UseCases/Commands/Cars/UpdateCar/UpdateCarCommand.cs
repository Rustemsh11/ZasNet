using MediatR;

namespace ZasNet.Application.UseCases.Commands.Cars.UpdateCar;

public record UpdateCarCommand(
        int Id,
        string Number,
        int Status,
        int? CarModelId
    ) : IRequest;

