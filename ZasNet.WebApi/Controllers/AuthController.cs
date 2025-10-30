using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZasNet.Application.UseCases.Queries.Authentication.GetAuthToken;

namespace ZasNet.WebApi.Controllers;

[Route("api/v1/[controller]/[action]")]
[ApiController]
public class AuthController(IMediator mediator): ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    public async Task<AccessTokenDto> GetAuthToken([FromBody] GetAuthTokenRequest getAuthTokenRequest, CancellationToken token)
    {
        return await mediator.Send(getAuthTokenRequest, token);
    }
}
