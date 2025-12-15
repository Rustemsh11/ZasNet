using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.Employees.GetDispetchers;

public record GetDispetchersRequest : IRequest<List<EmployeeDto>>;
