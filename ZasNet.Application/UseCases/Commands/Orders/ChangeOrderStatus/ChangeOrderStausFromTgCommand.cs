using MediatR;
using Telegram.Bot.Types;

namespace ZasNet.Application.UseCases.Commands.Orders.ChangeOrderStatus;

public record ChangeOrderStausFromTgCommand(string Secret, Update Update) : IRequest<ChangeOrderStausFromTgResult>;