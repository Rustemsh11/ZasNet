using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ZasNet.Application.Services;

namespace ZasNet.Infrastruture.Services;

public class JwtTokenGenerator(IOptions<AuthSettings> authSettings) : IJwtTokenGenerator
{
    public (DateTime Expires, string Token) Generate(int userId, string login, string roleName)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.Value.secretKey));
        var signinCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claimsList = new List<Claim>()
                        {
                            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                            new Claim(ClaimTypes.Name, login),
                            new Claim(ClaimTypes.Role, roleName)
                        };
        var tokeOptions = new JwtSecurityToken(
            claims: claimsList,
            expires: DateTime.Now.AddHours(12),
            signingCredentials: signinCredentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);

        return (tokeOptions.ValidTo, tokenString);
    }
}
