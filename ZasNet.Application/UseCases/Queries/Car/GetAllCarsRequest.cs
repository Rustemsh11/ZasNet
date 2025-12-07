using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.Car;

public record GetAllCarsRequest():IRequest<List<CarDto>>;
