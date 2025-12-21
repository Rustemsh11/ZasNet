using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;
using ZasNet.Domain;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Commands.Users.CreateUser;

public class CreateEmployeeHandler(IRepositoryManager repositoryManager,
    IPasswordHashService passwordHashService) : IRequestHandler<CreateEmployeeRequest>
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
        
        var buch = await employeeRepo.FindByCondition(c => c.RoleId == Constants.GeneralLedgerRole, false).CountAsync(cancellationToken);
        if(buch > 1)
        {
            throw new InvalidOperationException("Бухгалтер может быть только 1, если это новый бухгалтер, сначало удалите прежднего бугалтера");
        }

        var user = Employee.CreateEmployee(request.Name, request.Phone, request.Login, passwordHashService.HashPassword(request.Password), request.DispetcherProcent, role);

        employeeRepo.Create(user);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}
