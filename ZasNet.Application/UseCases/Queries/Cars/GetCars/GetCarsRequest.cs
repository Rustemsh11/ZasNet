using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.Cars.GetCars;

public record GetCarsRequest() : IRequest<List<CarDto>>;

