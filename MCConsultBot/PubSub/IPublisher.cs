using MCConsultBot.Telegram;

namespace MCConsultBot.PubSub
{
    interface IPublisher
    {
        void PublishEvent(TelegramBotClientFacade telegramBotClientFacade);
        void AddSubscriber(BaseSubscriber subscriber);
        void RemoveSubscriber(BaseSubscriber subscriber);
    }
}
