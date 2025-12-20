using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Domain;

namespace ZasNet.Application.UseCases.Queries.Employees.GetEmployees;

public class GetEmployeesHandler(IRepositoryManager repositoryManager) : IRequestHandler<GetEmployeesRequest, List<EmployeeDto>>
{
    public Task<List<EmployeeDto>> Handle(GetEmployeesRequest request, CancellationToken cancellationToken)
    {
        return repositoryManager.EmployeeRepository.FindByCondition(c=>c.RoleId != Constants.AdminRole, false)
            .Include(c=>c.Role)
            .Select(c => new EmployeeDto()
        {
            Id = c.Id,
            Name = c.Name,
            Login = c.Login,
            Password = c.Password,
            Role = new RoleDto()
            {
                Id = c.Role.Id,
                Name = c.Role.Name,
            }
        }).ToListAsync(cancellationToken);
    }
}
