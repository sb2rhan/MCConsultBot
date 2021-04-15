using MCConsultBot.PubSub;
using MCConsultBot.Telegram;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCConsultBot.State
{
    class SubscribeToDiscountsState : ICurrentState
    {
        public void PrerenderDefaultOutput(long chatId, 
            TelegramBotClientFacade telegramBotClientFacade)
        {
            var instance = SpecialDiscountEventPublisher.Instance;
            instance.AddSubscriber(new SpecialDiscountEventSubscriber(chatId));

            telegramBotClientFacade.SendTextMessageToChat(chatId,
                "Вы успешно подписались на новости нашего медцентра!");
        }

        public void ProcessInput(long chatId, 
            string input, 
            TelegramBotClientFacade telegramBotClientFacade, CurrentStateHolder stateHolder)
        {
            stateHolder.SetNextState(new OnStartSelectState());
        }
    }
}
