using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain;

namespace ZasNet.Application.UseCases.Commands.Users.DeleteEmployee;

public class DeleteEmployeeHandler(IRepositoryManager repositoryManager) : IRequestHandler<DeleteEmployeeCommand>
{
    public async Task Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        if(request.Id == Constants.UnknowingEmployeeId)
        {
            throw new ArgumentException("Нельзя удалить этого сотрудника");
        }

        var employee = await repositoryManager.EmployeeRepository.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException($"Сотрудника с id [{request.Id}] не найдено");

        employee.IsDeleted = true;

        await repositoryManager.SaveAsync(cancellationToken);
    }
}
