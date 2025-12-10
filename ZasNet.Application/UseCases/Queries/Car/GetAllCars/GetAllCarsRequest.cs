using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.Car.GetAllCars;

public record GetAllCarsRequest() : IRequest<List<CarDto>>;
