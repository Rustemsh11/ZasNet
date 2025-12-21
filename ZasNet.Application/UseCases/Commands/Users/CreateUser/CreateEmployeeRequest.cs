using MediatR;

namespace ZasNet.Application.UseCases.Commands.Users.CreateUser;

public record CreateEmployeeRequest(
        string Name,
        string? Phone,
        string Login,
        string Password,
        decimal? DispetcherProcent,
        int RoleId
    ) : IRequest;
