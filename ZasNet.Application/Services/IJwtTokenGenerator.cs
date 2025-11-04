namespace ZasNet.Application.Services;

public interface IJwtTokenGenerator
{
    (DateTime Expires, string Token) Generate(int userId, string login, string roleName);
}
