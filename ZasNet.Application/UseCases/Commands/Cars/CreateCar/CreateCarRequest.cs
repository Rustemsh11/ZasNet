using MediatR;

namespace ZasNet.Application.UseCases.Commands.Cars.CreateCar;

public record CreateCarRequest(
        string Number,
        int Status,
        int? CarModelId
    ) : IRequest;

