using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZasNet.Application.Repository;
using ZasNet.Application.Services.Telegram;
using ZasNet.Domain.Enums;
using ZasNet.Domain.Telegram;

namespace ZasNet.Infrastruture.Services;

public class OrderAutoProcessingService : BackgroundService
{
	private readonly IServiceScopeFactory serviceScopeFactory;
	private readonly ILogger<OrderAutoProcessingService> logger;
	private readonly ITelegramBotAnswerService telegramBotAnswer;

	public OrderAutoProcessingService(IServiceScopeFactory serviceScopeFactory, ILogger<OrderAutoProcessingService> logger, ITelegramBotAnswerService telegramBotAnswerService)
	{
		this.serviceScopeFactory = serviceScopeFactory;
		this.logger = logger;
		this.telegramBotAnswer = telegramBotAnswerService;
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
			.FindByCondition(o => o.Status == OrderStatus.ApprovedWithEmployers || o.Status == OrderStatus.Created && o.DateStart <= now, true)
			.Include(c=>c.CreatedEmployee)
			.ToListAsync(cancellationToken);

		if (dueOrders.Count == 0)
		{
			return;
		}

		foreach (var order in dueOrders)
		{
			//if(order.Status == OrderStatus.ApprovedWithEmployers)
			//{
			//	var buttons = new List<Button>
			//	{
			//		new Button { Text = "Открыть заявку", Url = $"https://localhost:7111/order/{order.Id}" }
			//	};
			//	await this.telegramBotAnswer.SendMessageAsync(order.CreatedEmployee.ChatId.Value, $"Началась работа по заявке [{order.Client}]. Необходимо подвтвердить выездны", buttons);
			//}
			order.UpdateStatus(OrderStatus.Processing);
			// tracked entities will be saved; Update is not required but safe if the entity wasn't tracked for any reason
			repositoryManager.OrderRepository.Update(order);
		}

		await repositoryManager.SaveAsync(cancellationToken);

		logger.LogInformation("Auto-switched {Count} order(s) to Processing", dueOrders.Count);
	}
}

