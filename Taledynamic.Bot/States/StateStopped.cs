﻿using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TaleDynamicBot.States
{
    public class StateStopped:State
    {

        public override void Auth(ITelegramBotClient botClient, Update update)
        { 
            botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "You are already logged into the system"
            );
        }

        public override void SendingData(ITelegramBotClient botClient, Message message)
        {
            
             botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Please,Wait"
            );
             this._user.ChangeState(new StateMessageHandling());
        }

        public override void StopSendingData(ITelegramBotClient botClient,Update update)
        {
            botClient.SendTextMessageAsync(
                chatId: update.Message.Chat.Id,
                text: "You already stopped"
            );
        }

        public override void DefaultAction(ITelegramBotClient botClient, Message message)
        {
            botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Какой-то текст"
            );
        }
    }
}