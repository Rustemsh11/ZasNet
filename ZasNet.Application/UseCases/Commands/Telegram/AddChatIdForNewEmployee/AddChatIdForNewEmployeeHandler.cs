using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.Telegram.AddChatIdForNewEmployee;

public class AddChatIdForNewEmployeeHandler(IRepositoryManager repositoryManager) : IRequestHandler<AddChatIdForNewEmployeeCommand>
{
    public async Task Handle(AddChatIdForNewEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await repositoryManager.EmployeeRepository.FindByCondition(c => c.Name == request.userName, true).SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException($"Пользователя с логином {request.userName}");
        
        employee.SetChatId(request.chatId);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}
