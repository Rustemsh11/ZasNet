using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Telegram.Bot;
using ZasNet.Application;
using ZasNet.Application.Repository;
using ZasNet.Application.Services;
using ZasNet.Infrastruture;
using ZasNet.Infrastruture.Persistence;
using ZasNet.Infrastruture.Repositories;
using ZasNet.Infrastruture.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ZasNetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqlConnection")));

builder.Services.AddScoped<IRepositoryManager, RepositoryManager>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
});
builder.Services.AddAutoMapper(typeof(ApplicationAssemblyMarker).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly);
builder.Services.Configure<AuthSettings>(builder.Configuration.GetSection(nameof(AuthSettings)));
builder.Services.Configure<TelegramSettings>(builder.Configuration.GetSection(nameof(TelegramSettings)));

var authSettings = builder.Configuration.GetSection(nameof(AuthSettings)).Get<AuthSettings>()
                      ?? throw new InvalidOperationException("AuthSettings not found");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
           .AddJwtBearer(options =>
           {
               options.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = false,
                   ValidateAudience = false,
                   IssuerSigningKey =
                       new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.secretKey))
               };
           });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<IOrderTelegramMessageBuilder, OrderTelegramMessageBuilder>();
builder.Services.AddSingleton<ITelegramValidate, TelegramValidate>();

var telegramSettings = builder.Configuration.GetSection(nameof(TelegramSettings)).Get<TelegramSettings>();
if (telegramSettings is not null && telegramSettings.IsEnabled)
{
    builder.Services.AddHttpClient("telegram-bot")
        .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
        {
            var options = sp.GetRequiredService<IOptions<TelegramSettings>>();
            return new TelegramBotClient(options.Value.BotToken, httpClient);
        });

    builder.Services.AddScoped<IOrderNotificationService, TelegramOrderNotificationService>();

    //var botClient = new TelegramBotClient(telegramSettings.BotToken);
    //var receiverOptions = new ReceiverOptions()
    //{
    //    AllowedUpdates = new[]
    //    {
    //        UpdateType.Message,
    //        UpdateType.CallbackQuery,
    //    },
    //    DropPendingUpdates = false,
    //};

    //using var cts = new CancellationTokenSource();

    //// Build a service provider to resolve MediatR and settings for Telegram update handling
    //var serviceProvider = builder.Services.BuildServiceProvider();
    //var mediator = serviceProvider.GetRequiredService<IMediator>();
    //var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    //var logger = loggerFactory.CreateLogger("TelegramBotPolling");
    //var telegramOptions = serviceProvider.GetRequiredService<IOptions<TelegramSettings>>();
    //var webhookSecret = telegramOptions.Value.WebhookSecret;

    //botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token);

    //async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        // Reuse existing MediatR command/handler to process Telegram updates
    //        var command = new ChangeOrderStausFromTgCommand(webhookSecret, update);
    //        var result = await mediator.Send(command, cancellationToken);

    //        logger.LogDebug("Telegram update handled with status code {StatusCode}", result.StatusCode);
    //    }
    //    catch (Exception ex)
    //    {
    //        logger.LogError(ex, "Error while handling Telegram update");
    //    }
    //}

    //Task ErrorHandler(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    //{
    //    logger.LogError(exception, "Telegram polling error");
    //    return Task.CompletedTask;
    //}
}
else
{
    builder.Services.AddScoped<IOrderNotificationService, NoOpOrderNotificationService>();
}


builder.Services.AddCors(opt =>
{
    opt.AddDefaultPolicy(builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
