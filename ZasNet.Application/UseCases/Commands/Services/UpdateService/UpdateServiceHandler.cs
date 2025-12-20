using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.Services.UpdateService;

public class UpdateServiceHandler(IRepositoryManager repositoryManager) : IRequestHandler<UpdateServiceCommand>
{
    public async Task Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
    {
        var serviceRepo = repositoryManager.ServiceRepository;

        var measure = await repositoryManager.MeasureRepository.FindByCondition(c => c.Id == request.MeasureId, false).SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException("Такой единицы измерения не существует");

        var service = await serviceRepo.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken);
        if (service == null)
        {
            throw new InvalidOperationException($"Услуги с id: {request.Id} не существует");
        }

        service.UpdateService(
            request.Name,
            request.Price,
            request.MeasureId,
            request.MinVolume,
            request.StandartPrecentForEmployee,
            request.PrecentForMultipleEmployeers,
            request.PrecentLaterOrderForEmployee,
            request.PrecentLaterOrderForMultipleEmployeers);

        serviceRepo.Update(service);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

