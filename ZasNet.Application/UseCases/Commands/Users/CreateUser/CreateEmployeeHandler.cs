using MediatR;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Commands.Users.CreateUser;

public class CreateEmployeeHandler(IRepositoryManager repositoryManager, IPasswordHashService passwordHashService) : IRequestHandler<CreateEmployeeRequest>
{
    public async Task Handle(CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var employeeRepo = repositoryManager.EmployeeRepository;

        var role = repositoryManager.RoleRepository.FindByCondition(c => c.Id == request.RoleId, false).SingleOrDefault() 
            ?? throw new InvalidOperationException("Такой роли не существует");

        var matchs = employeeRepo.FindByCondition(c => c.Login.ToLower() == request.Login.ToLower(), false).ToList();
        if(matchs.Count > 0)
        {
            throw new InvalidOperationException("Сотрудник с таким логином уже существует, выберите другой логин");
        }

        var user = Employee.CreateEmployee(request.Name, request.Phone, request.Login, passwordHashService.HashPassword(request.Password), role);

        employeeRepo.Create(user);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}
