using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.Car.GetAllActiveCars;

public class GetAllActiveCarsHandler(IRepositoryManager repositoryManager) : IRequestHandler<GetAllActiveCarsRequest, List<CarDto>>
{
    public async Task<List<CarDto>> Handle(GetAllActiveCarsRequest request, CancellationToken cancellationToken)
    {
        var cars = await repositoryManager.CarRepository.FindByCondition(c=> c.Status == Domain.Enums.CarStatus.Active, false).Include(c => c.CarModel).ToListAsync(cancellationToken);

        return cars.Select(c => new CarDto() { Id = c.Id, Name = $"{c.CarModel.Name}({c.Number})" }).ToList();
    }
}
