using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.CarModels.UpdateCarModel;

public class UpdateCarModelHandler(IRepositoryManager repositoryManager) : IRequestHandler<UpdateCarModelCommand>
{
    public async Task Handle(UpdateCarModelCommand request, CancellationToken cancellationToken)
    {
        var carModelRepo = repositoryManager.CarModelRepository;

        var carModel = await carModelRepo.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken);
        if (carModel == null)
        {
            throw new InvalidOperationException($"Модели автомобиля с id: {request.Id} не существует");
        }

        carModel.UpdateCarModel(request.Name);

        carModelRepo.Update(carModel);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

