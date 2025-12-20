using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Commands.CarModels.DeleteCarModel;

public class DeleteCarModelHandler(IRepositoryManager repositoryManager) : IRequestHandler<DeleteCarModelCommand>
{
    public async Task Handle(DeleteCarModelCommand request, CancellationToken cancellationToken)
    {
        var carModel = await repositoryManager.CarModelRepository.FindByCondition(c => c.Id == request.Id, true).SingleOrDefaultAsync(cancellationToken)
            ?? throw new ArgumentException($"Модели автомобиля с id [{request.Id}] не найдено");

        carModel.IsDeleted = true;

        await repositoryManager.SaveAsync(cancellationToken);
    }
}

