using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.Services.DeleteService;

public class DeleteServiceHandler(IRepositoryManager repositoryManager) : IRequestHandler<DeleteServiceCommand>
{
    public async Task Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await repositoryManager.ServiceRepository.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException($"Услуги с id [{request.Id}] не найдено");

        service.IsDeleted = true;

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

