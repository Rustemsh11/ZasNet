using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.EmployeeEarnings.EmployeeEarningUpdate;

public class EmployeeEarningUpdateHandler(IRepositoryManager repositoryManager) 
    : IRequestHandler<EmployeeEarningUpdateCommand>
{
    public async Task Handle(EmployeeEarningUpdateCommand request, CancellationToken cancellationToken)
    {
        var employeeEarning = await repositoryManager.EmployeeEarningRepository
            .FindByCondition(c => c.Id == request.EmployeeEarningDto.EmployeeEarningId, true)
            .SingleOrDefaultAsync(cancellationToken);

        if (employeeEarning == null)
        {
            throw new InvalidOperationException($"EmployeeEarning с ID {request.EmployeeEarningDto.EmployeeEarningId} не найден");
        }

        employeeEarning.Update(
            request.EmployeeEarningDto.ServiceEmployeePrecent,
            request.EmployeeEarningDto.PrecentEmployeeDescription,
            request.EmployeeEarningDto.EmployeeEarning
        );

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

