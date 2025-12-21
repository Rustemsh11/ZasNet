using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Commands.Users.UpdateEmployee;

public class UpdateEmployeeHandler(IRepositoryManager repositoryManager,
    IPasswordHashService passwordHashService) : IRequestHandler<UpdateEmployeeCommand>
{
    public async Task Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employeeRepo = repositoryManager.EmployeeRepository;

        var role = repositoryManager.RoleRepository.FindByCondition(c => c.Id == request.RoleId, false).SingleOrDefault()
            ?? throw new InvalidOperationException("Такой роли не существует");

        var employee = await employeeRepo.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken);
        if (employee == null)
        {
            throw new InvalidOperationException($"Сотрудника с id: {request.Id} не существует");
        }

        employee.UpdateEmployee(request.Name, request.Phone, request.Login, passwordHashService.HashPassword(request.Password), request.DispetcherProcent, role);

        employeeRepo.Update(employee);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}
