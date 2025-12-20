using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Commands.Services.CreateService;

public class CreateServiceHandler(IRepositoryManager repositoryManager) : IRequestHandler<CreateServiceRequest>
{
    public async Task Handle(CreateServiceRequest request, CancellationToken cancellationToken)
    {
        var serviceRepo = repositoryManager.ServiceRepository;

        var measure = await repositoryManager.MeasureRepository.FindByCondition(c => c.Id == request.MeasureId, false).SingleOrDefaultAsync(cancellationToken) 
            ?? throw new InvalidOperationException("Такой единицы измерения не существует");

        var service = Service.Create(
            request.Name,
            request.Price,
            request.MeasureId,
            request.MinVolume,
            request.StandartPrecentForEmployee,
            request.PrecentForMultipleEmployeers,
            request.PrecentLaterOrderForEmployee,
            request.PrecentLaterOrderForMultipleEmployeers);

        serviceRepo.Create(service);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

