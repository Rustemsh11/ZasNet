using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;

namespace ZasNet.Application.UseCases.Commands.CarModels.CreateCarModel;

public class CreateCarModelHandler(IRepositoryManager repositoryManager) : IRequestHandler<CreateCarModelRequest>
{
    public async Task Handle(CreateCarModelRequest request, CancellationToken cancellationToken)
    {
        var carModelRepo = repositoryManager.CarModelRepository;

        var matchs = await carModelRepo.FindByCondition(c => c.Name.ToLower() == request.Name.ToLower(), false).ToListAsync(cancellationToken);
        if(matchs.Count > 0)
        {
            throw new InvalidOperationException("Модель автомобиля с таким названием уже существует");
        }

        var carModel = CarModel.Create(request.Name);

        carModelRepo.Create(carModel);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

