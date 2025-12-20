using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Commands.Cars.UpdateCar;

public class UpdateCarHandler(IRepositoryManager repositoryManager) : IRequestHandler<UpdateCarCommand>
{
    public async Task Handle(UpdateCarCommand request, CancellationToken cancellationToken)
    {
        var carRepo = repositoryManager.CarRepository;

        if (request.CarModelId.HasValue)
        {
            var carModel = await repositoryManager.CarModelRepository
                .FindByCondition(c => c.Id == request.CarModelId.Value, false)
                .SingleOrDefaultAsync(cancellationToken);
            
            if (carModel == null)
            {
                throw new InvalidOperationException("Такой модели автомобиля не существует");
            }
        }

        var car = await carRepo.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken);
        if (car == null)
        {
            throw new InvalidOperationException($"Автомобиля с id: {request.Id} не существует");
        }

        car.UpdateCar(request.Number, (CarStatus)request.Status, request.CarModelId);

        carRepo.Update(car);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

