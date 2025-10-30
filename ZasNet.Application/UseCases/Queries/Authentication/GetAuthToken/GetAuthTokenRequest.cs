using MediatR;

namespace ZasNet.Application.UseCases.Queries.Authentication.GetAuthToken;

public record GetAuthTokenRequest(string Login, string Password): IRequest<AccessTokenDto>;
