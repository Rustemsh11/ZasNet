using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.Cars.DeleteCar;

public class DeleteCarHandler(IRepositoryManager repositoryManager) : IRequestHandler<DeleteCarCommand>
{
    public async Task Handle(DeleteCarCommand request, CancellationToken cancellationToken)
    {
        var car = await repositoryManager.CarRepository.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException($"Автомобиля с id [{request.Id}] не найдено");

        car.IsDeleted = true;

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

