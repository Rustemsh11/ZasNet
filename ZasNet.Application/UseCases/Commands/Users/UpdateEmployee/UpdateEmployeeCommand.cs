using MediatR;

namespace ZasNet.Application.UseCases.Commands.Users.UpdateEmployee;

public record UpdateEmployeeCommand(int Id, string Name, string? Phone, string Login, string Password, int RoleId) : IRequest;
