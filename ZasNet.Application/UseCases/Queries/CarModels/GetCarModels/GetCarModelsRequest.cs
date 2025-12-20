using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.CarModels.GetCarModels;

public record GetCarModelsRequest() : IRequest<List<CarModelDto>>;

