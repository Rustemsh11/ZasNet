using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.Employees.GetDrivers;

public record GetDriversRequest : IRequest<List<EmployeeDto>>;

