using MediatR;

namespace ZasNet.Application.UseCases.Commands.Users.CreateUser;

public record CreateUserRequest(
        string Login,
        string Password,
        int RoleId
    ) : IRequest;
