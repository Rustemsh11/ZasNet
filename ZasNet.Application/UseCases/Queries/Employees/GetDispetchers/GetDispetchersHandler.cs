using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Domain;

namespace ZasNet.Application.UseCases.Queries.Employees.GetDispetchers;

public class GetDispetchersHandler(IRepositoryManager repositoryManager) : IRequestHandler<GetDispetchersRequest, List<EmployeeDto>>
{
    public Task<List<EmployeeDto>> Handle(GetDispetchersRequest request, CancellationToken cancellationToken)
    {
        return repositoryManager.EmployeeRepository.FindByCondition(c => c.RoleId == Constants.DispetcherRole, false).Select(c=> new EmployeeDto()
        {
            Id = c.Id,
            Name = c.Name,
        }).ToListAsync(cancellationToken);
    }
}
