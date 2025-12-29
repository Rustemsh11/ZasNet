using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;

namespace ZasNet.Application.UseCases.Queries.Authentication.GetAuthToken;

public class GetAuthTokenHandler(IRepositoryManager repositoryManager,
    IPasswordHashService passwordHashService,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<GetAuthTokenRequest, AccessTokenDto>
{
    public async Task<AccessTokenDto> Handle(GetAuthTokenRequest request, CancellationToken cancellationToken)
    {
        var user = await repositoryManager.EmployeeRepository.FindByCondition(c => c.Login == request.Login, false).Include(c=>c.Role).SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException($"Пользователь с логином: {request.Login} не найден");

        if (!passwordHashService.VerifyHashedPassword(user.Password, request.Password))
        {
            throw new InvalidOperationException("Пароль не верный. Попробуйте еще раз");
        }

        var emoployee = await repositoryManager.EmployeeRepository.FindByCondition(c => c.Id == user.Id, false).Include(c=>c.Role).SingleOrDefaultAsync(cancellationToken);
        var employeeDto = new EmployeeDto() { Id = user.Id, Name = emoployee.Name, Role = new RoleDto() { Id = emoployee.RoleId, Name = emoployee.Role.Name } };
        var (expires, token) = jwtTokenGenerator.Generate(employeeDto, user.Login, user.Role.Name);

        return new AccessTokenDto() { Token = token, ExpiredTime = expires, User = employeeDto };
    }
}
