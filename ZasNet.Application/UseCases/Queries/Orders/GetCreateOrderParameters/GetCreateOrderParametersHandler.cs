using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.CommonDtos;
using ZasNet.Application.Repository;
using ZasNet.Domain.Enums;

namespace ZasNet.Application.UseCases.Queries.Orders.GetCreateOrderParameters;

public class GetCreateOrderParametersHandler(IRepositoryManager repositoryManager) : IRequestHandler<GetCreateOrderParametersRequest, CreateOrderParameters>
{
    public async Task<CreateOrderParameters> Handle(GetCreateOrderParametersRequest request, CancellationToken cancellationToken)
    {
        var employees = await repositoryManager.EmployeeRepository.FindByCondition(c=>c.Role.Name == "Водитель", false).Select(c=> new EmployeeDto() { Id = c.Id, Name = c.Name}).ToListAsync(cancellationToken);
        var services = await repositoryManager.ServiceRepository.FindAll(false).Include(c=>c.Measure).Select(c=>new ServiceDto() { Id = c.Id,
            Name = c.Name,
            MinPrice = c.Price,
            Measure = c.Measure.Name,
            StandartPrecentForEmployee = c.StandartPrecentForEmployee,
            PrecentLaterOrderForMultipleEmployeers = c.PrecentLaterOrderForMultipleEmployeers,
            PrecentForMultipleEmployeers = c.PrecentForMultipleEmployeers,
            PrecentLaterOrderForEmployee = c.PrecentLaterOrderForEmployee,
            MinVolume = c.MinVolume}).ToListAsync(cancellationToken);
        var cars = await repositoryManager.CarRepository.FindByCondition(c => c.Status == CarStatus.Active, false).Include(c=>c.CarModel).Select(c=> new CarDto() { Id = c.Id, Name = $"{c.CarModel.Name}({c.Number})" }).ToListAsync(cancellationToken);


        var paymentTypes = Enum.GetValues(typeof(PaymentType)).Cast<PaymentType>().ToList();

        return new CreateOrderParameters()
        {
            CarDtos = cars,
            EmployeeDtos = employees,
            ServiceDtos = services,
            PaymentTypes = paymentTypes!,
        };
    }
}
