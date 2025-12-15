using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.Services.GetServices;

public record GetServicesRequest : IRequest<List<ServiceDto>>;

