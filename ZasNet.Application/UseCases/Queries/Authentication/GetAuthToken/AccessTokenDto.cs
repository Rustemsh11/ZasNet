namespace ZasNet.Application.UseCases.Queries.Authentication.GetAuthToken;

public class AccessTokenDto
{
    public string Token { get; set;}
    
    public DateTime ExpiredTime { get; set;}
}
