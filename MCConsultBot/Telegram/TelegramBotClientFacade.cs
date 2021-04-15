using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace MCConsultBot.Telegram
{
    public class TelegramBotClientFacade
    {
        private readonly ITelegramBotClient _telegramBotClient;

        public TelegramBotClientFacade(
            ITelegramBotClient telegramBotClient)
        {
            _telegramBotClient = telegramBotClient ?? 
                throw new ArgumentNullException(nameof(telegramBotClient));
        }

        public void SendMapMessageToChat(
            long chatId)
        {
            _telegramBotClient.SendVenueAsync(
                chatId: chatId,
                latitude: 50.0840172f,
                longitude: 14.418288f,
                title: "Клиника 'Дешево и сердито'",
                address: "Алматы, ул. Жарокова 137")
                .GetAwaiter()
                .GetResult();
        }

        public void SendTextMessageToChat(
            long chatId,
            string textMessage)
        {
            _telegramBotClient
                .SendTextMessageAsync(chatId, textMessage)
                .GetAwaiter()
                .GetResult();
        }

        public void SendButtonMessageToChat(
            long chatId,
            IEnumerable<string> buttons)
        {
            // Оборачиваем сложный API библиотеки ITelegramBotClient в удобный
            // метод SendButtonMessageToChat для отправки кнопок клиенту

            var keyboardButtons = buttons
                .Select(p => new KeyboardButton() { Text = p });

            var replyMarkup = new ReplyKeyboardMarkup(keyboardButtons);

            _telegramBotClient.SendTextMessageAsync(
                chatId,
                "Выберите опцию",
                replyMarkup: replyMarkup)
                .GetAwaiter()
                .GetResult();
        }
    }
}
