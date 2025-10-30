namespace ZasNet.Application.Services;

public interface IJwtTokenGenerator
{
    (DateTime Expires, string Token) Generate(string login, string roleName);
}
