using MCConsultBot.Telegram;
using System.Collections.Generic;
using Telegram.Bot;

namespace MCConsultBot.Infrastructure
{
    /// <summary>
    /// Fluent Builder (порождающие паттерны) -
    /// Configuration, 
    /// Builder, 
    /// Factory, 
    /// Singleton etc...
    /// </summary>
    class ProcessingMiddlewareBuilder
    {
        private ICollection<MiddlewareComponentBase> _middlewareComponents;
        private TelegramBotClientFacade _client;
        public ProcessingMiddlewareBuilder()
        {
            _middlewareComponents = new List<MiddlewareComponentBase>();
        }

        public ProcessingMiddlewareBuilder AddComponent(
            MiddlewareComponentBase middlewareComponentBase)
        {
            _middlewareComponents.Add(middlewareComponentBase);
            return this;
        }

        public ProcessingMiddlewareBuilder AddTelegramClient(
            TelegramBotClientFacade telegramBotClientFacade)
        {
            _client = telegramBotClientFacade;
            return this;
        }

        public ProcessingMiddlewareBuilder AddTelegramClient(
            ITelegramBotClient telegramBotClient)
        {
            _client = new TelegramBotClientFacade(telegramBotClient);
            return this;
        }

        public ProcessingMiddleware Build()
        {
            var processingMiddleware = new ProcessingMiddleware(_client, _middlewareComponents);
            return processingMiddleware;
        }
    }
}
