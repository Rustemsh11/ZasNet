using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using ZasNet.Application.Services.Telegram;
using ZasNet.Domain.Telegram;
using ZasNet.Infrastruture.Services.Telegram;

namespace ZasNet.WebApi.Controllers;

[ApiController]
[Route("telegram")]
public class TelegramController(IMediator mediator,
    TelegramMessageProcessor messageProcessor,
    ITelegramBotAnswerService telegramBotService) : ControllerBase
{
    private readonly TelegramMessageProcessor messageProcessor = messageProcessor;
    private readonly ITelegramBotAnswerService telegramBotService = telegramBotService;
    private readonly IMediator _mediator = mediator;

    [HttpPost("update")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveUpdate([FromBody] Update update, CancellationToken cancellationToken)
    {
        var secret = Request.Headers["X-Telegram-Bot-Api-Secret-Token"].FirstOrDefault();
        if(update == null)
        {
            return BadRequest("Обновление не может быть пустым");
        }

        var result = await messageProcessor.ProcessAsync(new TelegramUpdate()
        {
            UpdateId = update.Id,
            Message = new TelegramMessage()
            {
                Date = update.Message.Date,
                From = new TelegramUser()
                {
                    ChatId = update.Message.Chat.Id,
                    Id = update.Message.Id,
                    Username = update.Message?.From?.Username, 
                },
                MessageId = update.Message.MessageId,
                Text = update.Message.Text,
                Photo = update.Message.Photo?.Select(c=> new TelegramPhoto()
                {
                    FileId = c.FileId,
                    FileSize = c.FileSize,
                    Height = c.Height,
                    Width = c.Width,
                    FileUniqueId = c.FileUniqueId,
                }).ToArray(),

            },
            CallbackQuery = update.CallbackQuery == null ? null : new TelegramCallbackQuery()
            {
                Data = update.CallbackQuery.Data,
                From = new TelegramUser()
                {
                    ChatId = update.CallbackQuery.Message.Chat.Id,
                    Id = update.CallbackQuery.Message.Id,
                    Username = update.CallbackQuery.Message.From.Username,
                },
                Id = update.CallbackQuery.Id,
                Message = new TelegramMessage()
                {
                    Date = update.CallbackQuery.Message.Date,
                    From = new TelegramUser()
                    {
                        ChatId = update.CallbackQuery.Message.Chat.Id,
                        Id = update.CallbackQuery.Message.Id,
                        Username = update.CallbackQuery.Message.From.Username,
                    },
                    MessageId = update.CallbackQuery.Message.MessageId,
                    Text = update.CallbackQuery.Message.Text,
                    Photo = update.CallbackQuery.Message.Photo?.Select(c => new TelegramPhoto()
                    {
                        FileId = c.FileId,
                        FileSize = c.FileSize,
                        Height = c.Height,
                        Width = c.Width,
                        FileUniqueId = c.FileUniqueId,
                    }).ToArray(),

                },
            },
        }, cancellationToken);

        if (!result.Success)
        {
            if (update.Message != null && !string.IsNullOrEmpty(result.ResponseMessage))
            {
                await telegramBotService.SendMessageAsync(
                    update.Message.Chat.Id,
                    result.ResponseMessage,
                    cancellationToken);
            }

            return Ok();
        }

        // Отправляем ответ пользователю
        if (!string.IsNullOrEmpty(result.ResponseMessage))
        {
            long chatId = 0;

            // Определяем chatId в зависимости от типа обновления
            if (update.Message != null)
            {
                chatId = update.Message.Chat.Id;
                await telegramBotService.SendMessageAsync(chatId, result.ResponseMessage, cancellationToken);
            }
            else if (update.CallbackQuery != null)
            {
                // Отвечаем на callback query
                await telegramBotService.AnswerCallbackQueryAsync(
                    update.CallbackQuery.Id,
                    result.ResponseMessage,
                    cancellationToken);

                // Если есть сообщение, отправляем обновленное сообщение
                if (update.CallbackQuery.Message != null)
                {
                    chatId = update.CallbackQuery.Message.Chat.Id;
                    await telegramBotService.SendMessageAsync(chatId, result.ResponseMessage, cancellationToken);
                }
            }
        }

        return Ok();
    }
}

