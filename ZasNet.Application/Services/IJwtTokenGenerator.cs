using ZasNet.Application.CommonDtos;

namespace ZasNet.Application.Services;

public interface IJwtTokenGenerator
{
    (DateTime Expires, string Token) Generate(EmployeeDto user, string login, string roleName);
}
