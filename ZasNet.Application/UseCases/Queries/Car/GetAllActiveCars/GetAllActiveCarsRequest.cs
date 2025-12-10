using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.Car.GetAllActiveCars;

public record GetAllActiveCarsRequest() : IRequest<List<CarDto>>;
