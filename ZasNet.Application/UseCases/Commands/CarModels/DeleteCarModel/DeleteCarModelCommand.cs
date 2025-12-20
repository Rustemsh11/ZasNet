using MediatR;

namespace ZasNet.Application.UseCases.Commands.CarModels.DeleteCarModel;

public record DeleteCarModelCommand(int Id): IRequest;

