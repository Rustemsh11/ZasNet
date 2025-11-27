using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZasNet.Application.Repository;
using ZasNet.Domain.Enums;

namespace ZasNet.Infrastruture.Services;

public class OrderAutoProcessingService : BackgroundService
{
	private readonly IServiceScopeFactory serviceScopeFactory;
	private readonly ILogger<OrderAutoProcessingService> logger;

	public OrderAutoProcessingService(IServiceScopeFactory serviceScopeFactory, ILogger<OrderAutoProcessingService> logger)
	{
		this.serviceScopeFactory = serviceScopeFactory;
		this.logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Order auto-processing service started");

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await ProcessDueOrders(stoppingToken);
			}
			catch (OperationCanceledException)
			{
				// shutdown requested
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error while processing due orders");
			}

			try
			{
				await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
			}
			catch (OperationCanceledException)
			{
				break;
			}
		}

		logger.LogInformation("Order auto-processing service stopped");
	}

	private async Task ProcessDueOrders(CancellationToken cancellationToken)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var repositoryManager = scope.ServiceProvider.GetRequiredService<IRepositoryManager>();

		var now = DateTime.Now;

		var dueOrders = await repositoryManager
			.OrderRepository
			.FindByCondition(o => o.Status == OrderStatus.ApprovedWithEmployers && o.Date <= now, true)
			.ToListAsync(cancellationToken);

		if (dueOrders.Count == 0)
		{
			return;
		}

		foreach (var order in dueOrders)
		{
			order.UpdateStatus(OrderStatus.Processing);
			// tracked entities will be saved; Update is not required but safe if the entity wasn't tracked for any reason
			repositoryManager.OrderRepository.Update(order);
		}

		await repositoryManager.SaveAsync(cancellationToken);

		logger.LogInformation("Auto-switched {Count} order(s) to Processing", dueOrders.Count);
	}
}

