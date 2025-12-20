using MediatR;

namespace ZasNet.Application.UseCases.Commands.CarModels.CreateCarModel;

public record CreateCarModelRequest(string Name) : IRequest;

