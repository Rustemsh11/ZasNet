using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Domain;

namespace ZasNet.Application.UseCases.Queries.Employees.GetDrivers;

public class GetDriversHandler(IRepositoryManager repositoryManager) : IRequestHandler<GetDriversRequest, List<EmployeeDto>>
{
    public Task<List<EmployeeDto>> Handle(GetDriversRequest request, CancellationToken cancellationToken)
    {
        return repositoryManager.EmployeeRepository.FindByCondition(c => c.RoleId == Constants.DriversRole, false).Select(c=> new EmployeeDto()
        {
            Id = c.Id,
            Name = c.Name,
        }).ToListAsync(cancellationToken);
    }
}

