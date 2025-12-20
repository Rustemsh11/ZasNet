using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.Cars.GetCars;

public class GetCarsHandler(IRepositoryManager repositoryManager) : IRequestHandler<GetCarsRequest, List<CarDto>>
{
    public async Task<List<CarDto>> Handle(GetCarsRequest request, CancellationToken cancellationToken)
    {
        return await repositoryManager.CarRepository.FindAll(false)
            .Include(c => c.CarModel)
            .Select(c => new CarDto 
            { 
                Id = c.Id, 
                Number = c.Number, 
                Status = (int)c.Status,
                CarModel = c.CarModel != null ? new CarModelDto 
                { 
                    Id = c.CarModel.Id, 
                    Name = c.CarModel.Name 
                } : null
            })
            .ToListAsync(cancellationToken);
    }
}

