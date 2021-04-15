using MCConsultBot.Database.Repositories;
using MCConsultBot.Telegram;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Args;

namespace MCConsultBot.Infrastructure
{
    /// <summary>
    /// Базовый класс для каждого этапа обработчика запроса
    /// </summary>
    abstract class MiddlewareComponentBase
    {
        public abstract void ProcessRequest(
            TelegramBotClientFacade client, 
            MessageEventArgs message);
    }

    class IncomingMessageLogger : MiddlewareComponentBase
    {
        public override void ProcessRequest(
            TelegramBotClientFacade client,
            MessageEventArgs messageEventArgs)
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            var chatId = messageEventArgs.Message.Chat.Id;
            var userName = messageEventArgs.Message.Chat.Username;

            var lotItem = $"Поступило сообщение от пользователя {userName} в чате {chatId}";
            logger.Information(lotItem);
        }
    }

    class IncomingMessageThrottler : MiddlewareComponentBase
    {
        private readonly int _messageLimitPerMinute;
        private readonly IRateLimiterRepository _rateLimiterRepository;
        private DateTime _lastRefreshTime;

        public IncomingMessageThrottler(
            IRateLimiterRepository rateLimiterRepository,
            int messageLimitPerMinute = 10)
        {
            _rateLimiterRepository = rateLimiterRepository;
            _messageLimitPerMinute = messageLimitPerMinute;
            _lastRefreshTime = DateTime.Now;

            // RefreshOldRecords();
        }

        public override void ProcessRequest(
            TelegramBotClientFacade client,
            MessageEventArgs args)
        {
            var key = GetCurrentKey(args.Message.Chat.Id, DateTime.Now);
            _rateLimiterRepository.Increase(key);

            var currentCounter = _rateLimiterRepository.Get(key);
            if (currentCounter > _messageLimitPerMinute)
            {
                throw new InvalidOperationException("You have reached the limit of messages. Wait...");
            }

            RefreshOldRecords();
        }

        private void RefreshOldRecords()
        {
            var now = DateTime.Now;
            var passedPeriod = now - _lastRefreshTime;
            
            if(TimeSpan.FromMinutes(60) <= passedPeriod)
            {
                _rateLimiterRepository.Refresh();
                _lastRefreshTime = now;
            }
        }

        private string GetCurrentKey(long chatId, DateTime now) => $"{chatId}_{now.Minute}";
    }


    /// <summary>
    /// Здесь начинается непосредственно бизнес-логика
    /// </summary>
    class MessageProcessingLogic : MiddlewareComponentBase
    {
        private readonly InMemoryStateRepository _stateRepository;
        public MessageProcessingLogic(
            InMemoryStateRepository stateRepository)
        {
            _stateRepository = stateRepository;
        }

        public override void ProcessRequest(
            TelegramBotClientFacade client,
            MessageEventArgs args)
        {
            var chatId = args.Message.Chat.Id;
            var state = _stateRepository.Get(chatId);

            state.ProcessInput(chatId, args.Message.Text, client);
        }
    }
}
