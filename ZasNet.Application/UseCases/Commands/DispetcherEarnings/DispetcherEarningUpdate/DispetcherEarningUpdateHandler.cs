using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.DispetcherEarnings.DispetcherEarningUpdate;

public class DispetcherEarningUpdateHandler(IRepositoryManager repositoryManager) 
    : IRequestHandler<DispetcherEarningUpdateCommand>
{
    public async Task Handle(DispetcherEarningUpdateCommand request, CancellationToken cancellationToken)
    {
        var dispetcherEarning = await repositoryManager.DispetcherEarningRepository
            .FindByCondition(c => c.Id == request.EmployeeEarningDto.EmployeeEarningId, true)
            .SingleOrDefaultAsync(cancellationToken);

        if (dispetcherEarning == null)
        {
            throw new InvalidOperationException($"DispetcherEarning с ID {request.EmployeeEarningDto.EmployeeEarningId} не найден");
        }

        dispetcherEarning.Update(
            request.EmployeeEarningDto.ServiceEmployeePrecent,
            request.EmployeeEarningDto.PrecentEmployeeDescription,
            request.EmployeeEarningDto.EmployeeEarning
        );

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

