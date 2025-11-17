using MediatR;

namespace ZasNet.Application.UseCases.Commands.Telegram.AddChatIdForNewEmployee;

public record AddChatIdForNewEmployeeCommand(long chatId, string userName) : IRequest;
