using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Serilog;
using TaleDynamicBot.States;

namespace TaleDynamicBot
{
    public class Handlers
    {
        public static User user = new User(new StateNonAuth());

        public static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception,
            CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException =>
                    $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Log.Error(errorMessage);
            return Task.CompletedTask;
        }

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update,
            CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message            => BotOnMessageReceived(botClient, update.Message!),
                UpdateType.CallbackQuery      => BotOnCallbackQueryReceived(botClient, update.CallbackQuery!),
                _ => throw new ArgumentOutOfRangeException()
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        public static async Task BotOnCallbackQueryReceived(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {
            await user.CallbackQueryHandler(botClient, callbackQuery);
        }

        public static async Task BotOnMessageReceived(ITelegramBotClient botClient, Message message)
        {
            using (var helper = new ApiHelper(
                botClient,
                "http://localhost:5000",
                message?.From?.Id.ToString(), message?.From?.Username?.ToString()))
            {
                await helper.SaveMessage(botClient, message);
            }
        }
        static async Task<Message> Usage(ITelegramBotClient botClient, Message message)
        {
            const string usage = "Usage:\n" +
                                 "/auth     - authorize in project\n" +
                                 "/sending  - start sending message on TaleDynamic\n"+
                                 "/stop_sending - stop receiving your messages\n";

            return await botClient.SendTextMessageAsync(chatId: message.Chat.Id, 
                text: usage,
                replyMarkup: new ReplyKeyboardRemove());
        }
    }
}