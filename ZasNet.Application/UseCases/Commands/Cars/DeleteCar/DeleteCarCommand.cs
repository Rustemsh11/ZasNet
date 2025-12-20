using MediatR;

namespace ZasNet.Application.UseCases.Commands.Cars.DeleteCar;

public record DeleteCarCommand(int Id): IRequest;

