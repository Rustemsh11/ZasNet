using Microsoft.AspNetCore.Http;

namespace ZasNet.Application.UseCases.Commands.Orders.ChangeOrderStatus;

public record ChangeOrderStausFromTgResult(int StatusCode)
{
    public static ChangeOrderStausFromTgResult Success() => new(StatusCodes.Status200OK);
    public static ChangeOrderStausFromTgResult Unauthorized() => new(StatusCodes.Status401Unauthorized);
    public static ChangeOrderStausFromTgResult ServiceUnavailable() => new(StatusCodes.Status503ServiceUnavailable);
}
