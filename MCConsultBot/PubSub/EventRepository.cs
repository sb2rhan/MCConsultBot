using MCConsultBot.Telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MCConsultBot.PubSub
{
    class EventRepository
    {
        private Dictionary<IPublisher, DateTime> _nextRunTime;
        private Dictionary<IPublisher, TimeSpan> _periodTable;
        private Task _workerThread;

        public EventRepository()
        {
            _nextRunTime = new Dictionary<IPublisher, DateTime>();
            _periodTable = new Dictionary<IPublisher, TimeSpan>();
        }

        /// <summary>
        /// Пусть задача {publisher} запускается с интервалом {period}
        /// </summary>
        public void AddScheduleToEvent(
            IPublisher publisher, 
            TimeSpan period)
        {
            var now = DateTime.Now;
            var nextRun = now + period;

            _nextRunTime[publisher] = nextRun;
            _periodTable[publisher] = period;
        }

        private IEnumerable<IPublisher> GetEventsToPublishNow()
        {
            // next_run_times
            //    IPublisher | 01-01-2022 13:40:35

            // 01-01-2022 13:40:36

            var now = DateTime.Now;

            var eventsToPublish = 
                _nextRunTime.Where(p => (Math.Abs((p.Value - now).TotalSeconds)) < 2);

            var response = eventsToPublish.Select(p => p.Key).ToArray();

            for (int i = 0; i < response.Length; i++)
            {
                _nextRunTime[response[i]] = now + _periodTable[response[i]];
            }

            return response;
        }

        public void RunWorker(TelegramBotClientFacade telegramBotClientFacade)
        {
            _workerThread = Task.Run(() =>
            {
                while (true)
                {
                    var eventsToRun = GetEventsToPublishNow();

                    foreach (var item in eventsToRun)
                    {
                        Console.WriteLine($"READY TO PUBLISH : {item.GetType().Name}");
                        item.PublishEvent(telegramBotClientFacade);
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(1));
                }
            });
        }
    }
}
