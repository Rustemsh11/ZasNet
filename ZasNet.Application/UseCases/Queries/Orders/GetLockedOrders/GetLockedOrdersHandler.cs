using MediatR;
using Microsoft.EntityFrameworkCore;
using ZasNet.Application.Repository;

namespace ZasNet.Application.UseCases.Queries.Orders.GetLockedOrders;

public class GetLockedOrdersHandler(IRepositoryManager repositoryManager) 
	: IRequestHandler<GetLockedOrdersRequest, List<GetLockedOrdersResponse>>
{
	public async Task<List<GetLockedOrdersResponse>> Handle(GetLockedOrdersRequest request, CancellationToken cancellationToken)
	{
		// Получаем все заблокированные заявки
		var lockedOrders = await repositoryManager.OrderRepository
			.FindByCondition(o => o.LockedByUserId != null, false)
			.ToListAsync(cancellationToken);

		// Получаем уникальные ID сотрудников, которые заблокировали заявки
		var employeeIds = lockedOrders
			.Where(o => o.LockedByUserId.HasValue)
			.Select(o => o.LockedByUserId!.Value)
			.Distinct()
			.ToList();

		// Получаем информацию о сотрудниках
		var employees = await repositoryManager.EmployeeRepository
			.FindByCondition(e => employeeIds.Contains(e.Id), false)
			.ToDictionaryAsync(e => e.Id, cancellationToken);

		// Формируем результат
		var result = lockedOrders.Select(o => new GetLockedOrdersResponse
		{
			Id = o.Id,
			OrderClientName = o.Client,
			OrderDate = o.DateStart,
			LockedByUserName = o.LockedByUserId.HasValue && employees.TryGetValue(o.LockedByUserId.Value, out var employee)
				? employee.Name
				: null,
			LockedAt = o.LockedAt
		}).ToList();

		return result;
	}
}

