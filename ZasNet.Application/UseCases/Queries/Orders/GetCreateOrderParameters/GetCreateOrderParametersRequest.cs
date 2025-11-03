using MediatR;

namespace ZasNet.Application.UseCases.Queries.Orders.GetCreateOrderParameters;

public record GetCreateOrderParametersRequest(): IRequest<CreateOrderParameters>;
