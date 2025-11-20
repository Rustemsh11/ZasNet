using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types.Payments;
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
builder.Services.AddScoped<IMessageTypeResolver, MessageTypeResolver>();
builder.Services.AddScoped<ITelegramBotAnswerService, TelegramBotAnswerService>();

builder.Services.AddScoped<ITelegramMessageHandler, StartCommandHandler>();
builder.Services.AddScoped<ITelegramMessageHandler, SaveUserChatHandler>();

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

    builder.Services.AddScoped<IOrderNotificationService, TelegramOrderNotificationService>();
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
app.UseForwardedHeaders();
//app.UseHttpsRedirection(); enable on deploying!!!
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
