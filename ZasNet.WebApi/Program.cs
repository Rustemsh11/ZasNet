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
using ZasNet.Application.Services.Telegram;
using ZasNet.Application.Services.Telegram.Handlers;
using ZasNet.Domain.Interfaces;
using ZasNet.Infrastruture;
using ZasNet.Infrastruture.Persistence;
using ZasNet.Infrastruture.Repositories;
using ZasNet.Infrastruture.Services;
using ZasNet.Infrastruture.Services.Telegram;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ZasNetDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("sqlConnection")));

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

builder.Services.AddSingleton<ITelegramValidate, TelegramValidate>();
builder.Services.AddTransient<ITelegramBotAnswerService, TelegramBotAnswerService>();
builder.Services.AddSingleton<IFreeOrdersCache, FreeOrdersCache>();
builder.Services.AddSingleton<IUserSessionCache, UserSessionCache>();
builder.Services.AddScoped<IOrderServiceEmployeeApprovalService, OrderServiceEmployeeApprovalService>();
builder.Services.AddScoped<ITelegramFileService, TelegramFileService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IEmployeeEarningReportService, EmployeeEarningReportService>();
builder.Services.AddScoped<IDispetcherEarningReportService, DispetcherEarningReportService>();

builder.Services.AddScoped<ITelegramMessageHandler, StartCommandHandler>();
builder.Services.AddScoped<ITelegramMessageHandler, SaveUserChatHandler>();
builder.Services.AddScoped<ITelegramMessageHandler, MyOpenOrdersHandler>();
builder.Services.AddScoped<ITelegramMessageHandler, FreeOrdersHandler>();
builder.Services.AddScoped<ITelegramMessageHandler, MyProcessingOrdersHandler>();
builder.Services.AddScoped<ITelegramMessageHandler, AssignEmployeeToOrderServiceEmployeeHandler>();
builder.Services.AddScoped<ITelegramMessageHandler, ApproveAssinedNewOrderByEmployee>();
builder.Services.AddScoped<ITelegramMessageHandler, RejectAssignedNewOrderByEmployee>();
builder.Services.AddScoped<ITelegramMessageHandler, RefreshMenuHandler>();
builder.Services.AddScoped<ITelegramMessageHandler, ApproveAssinedCarByEmployee>();
builder.Services.AddScoped<ITelegramMessageHandler, ChangeOrderServiceCarsHandler>();
builder.Services.AddScoped<ITelegramMessageHandler, ChangeOrderServiceEmployeesHandler>();
builder.Services.AddScoped<ITelegramMessageHandler, ProcessingOrderEditHandler>();
builder.Services.AddScoped<ITelegramMessageHandler, GetNeedInvoiceOrdersHandler>();
builder.Services.AddScoped<ITelegramMessageHandler, AddOrderServiceHandler>();
builder.Services.AddScoped<ITelegramMessageHandler, DeleteOrderServiceHandler>();

builder.Services.AddScoped<TelegramMessageProcessor>();

var telegramSettings = builder.Configuration.GetSection(nameof(TelegramSettings)).Get<TelegramSettings>();
if (telegramSettings is not null && telegramSettings.IsEnabled)
{
    //tuna http 5142
    //

    //    $TOKEN = "<BOT_TOKEN>"
    //$PUBLIC_URL = "https://<subdomain>.tuna.am/telegram/update"
    //$SECRET = "<YOUR_SECRET>"
    //Invoke-RestMethod -Uri "https://api.telegram.org/bot$TOKEN/setWebhook" -Method Post -Body @{ url = $PUBLIC_URL; secret_token = $SECRET }
    //    Invoke-RestMethod -Uri "https://api.telegram.org/bot$TOKEN/getWebhookInfo"

    builder.Services.AddHttpClient("telegram-bot")
    .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
    {
            var options = sp.GetRequiredService<IOptions<TelegramSettings>>();
            return new TelegramBotClient(options.Value.BotToken, httpClient);
        });

    builder.Services.AddScoped<IOrderNotificationService, OrderNotificationService>();
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

// Background services
builder.Services.AddHostedService<OrderAutoProcessingService>();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors();
app.UseForwardedHeaders();
//app.UseHttpsRedirection(); enable on deploying!!!
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
