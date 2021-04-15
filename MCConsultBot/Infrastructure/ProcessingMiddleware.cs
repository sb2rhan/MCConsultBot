using MCConsultBot.Telegram;
using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Args;

namespace MCConsultBot.Infrastructure
{
    class ProcessingMiddleware
    {
        private readonly TelegramBotClientFacade _client;
        private readonly IEnumerable<MiddlewareComponentBase> _middlewareComponents;
        private readonly Logger _logger;

        public ProcessingMiddleware(
            TelegramBotClientFacade client, 
            IEnumerable<MiddlewareComponentBase> middlewareComponents)
        {
            _client = client ?? 
                throw new ArgumentNullException(nameof(client));

            _middlewareComponents = middlewareComponents ?? 
                throw new ArgumentNullException(nameof(middlewareComponents));

            _logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();
        }

        public void ProcessRequest(MessageEventArgs message)
        {
            try
            {
                // Pipeline processing (chain of responsibility)
                foreach (var component in _middlewareComponents)
                {
                    component.ProcessRequest(_client, message);
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex.ToString());
            }
        }
    }
}
