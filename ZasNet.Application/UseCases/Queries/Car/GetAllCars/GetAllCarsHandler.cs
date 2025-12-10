using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.Car.GetAllCars;

public class GetAllCarsHandler(IRepositoryManager repositoryManager) : IRequestHandler<GetAllCarsRequest, List<CarDto>>
{
    public async Task<List<CarDto>> Handle(GetAllCarsRequest request, CancellationToken cancellationToken)
    {
        var cars = await repositoryManager.CarRepository.FindAll(false).Include(c => c.CarModel).ToListAsync(cancellationToken);

        return cars.Select(c => new CarDto() { Id = c.Id, Name = $"{c.CarModel.Name}({c.Number})" }).ToList();
    }
}
