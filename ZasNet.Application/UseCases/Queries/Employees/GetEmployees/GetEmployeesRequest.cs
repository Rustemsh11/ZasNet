using MediatR;
using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.UseCases.Queries.Employees.GetEmployees;

public record GetEmployeesRequest: IRequest<List<EmployeeDto>>;
