using MediatR;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Commands.Users.CreateUser;

public class CreateUserHandler(IRepositoryManager repositoryManager, IPasswordHashService passwordHashService) : IRequestHandler<CreateUserRequest>
{
    public async Task Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var userRepo = repositoryManager.UserRepository;

        var role = repositoryManager.RoleRepository.FindByCondition(c => c.Id == request.RoleId, false).SingleOrDefault() 
            ?? throw new InvalidOperationException("Такой роли не существует");

        var user = User.CreateUser(request.Login, passwordHashService.HashPassword(request.Password), role.Id);

        userRepo.Create(user);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}
