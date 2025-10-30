namespace ZasNet.Application.Services;

public interface IPasswordHashService
{
    string HashPassword(string password);
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}
