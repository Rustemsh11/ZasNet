using MediatR;

namespace ZasNet.Application.UseCases.Commands.CarModels.UpdateCarModel;

public record UpdateCarModelCommand(int Id, string Name) : IRequest;

