using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Commands.Cars.CreateCar;

public class CreateCarHandler(IRepositoryManager repositoryManager) : IRequestHandler<CreateCarRequest>
{
    public async Task Handle(CreateCarRequest request, CancellationToken cancellationToken)
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

        var matchs = await carRepo.FindByCondition(c => c.Number.ToLower() == request.Number.ToLower(), false).ToListAsync(cancellationToken);
        if(matchs.Count > 0)
        {
            throw new InvalidOperationException("Автомобиль с таким номером уже существует");
        }

        var car = Car.Create(request.Number, (CarStatus)request.Status, request.CarModelId);

        carRepo.Create(car);

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

