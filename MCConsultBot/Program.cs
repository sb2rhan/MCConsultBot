using MCConsultBot.Database;
using MCConsultBot.Database.Repositories;
using MCConsultBot.Infrastructure;
using MCConsultBot.PubSub;
using System;

using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using System.Collections.Generic;

namespace MCConsultBot
{
    class Program
    {
        static void InitializeDbRedis()
        {
            RedisDbManager db = RedisDbManager.Instance;


            string doctors = "doctors";
            if (db.CheckForKey(doctors))
                db.DeleteKey(doctors);

            db.SetValues(doctors, "Осипова Татьяна", "Амангельдиев Оспан", "Виталий Ким");

            // setting Options for OnStartSelectState
            string startName = "onstart_buttons";
            if (db.CheckForKey(startName))
                db.DeleteKey(startName);

            db.SetValues(startName,
                "Показать расписание врача",
                "Показать кабинет врача",
                "Показать филиалы медцентра",
                "Показать стоимость приема врача",
                "Подписаться на акции и скидки");

            // setting Doctors-Rooms for ShowDoctorRoomState
            string doctor_room = "doctor_room";
            if (db.CheckForKey(doctor_room))
                db.DeleteKey(doctor_room);

            db.SetValues(doctor_room,
                new KeyValuePair<string, string>("Осипова Татьяна", "232"),
                new KeyValuePair<string, string>("Амангельдиев Оспан", "236"),
                new KeyValuePair<string, string>("Виталий Ким", "105"));

            // setting Doctor-Schedule for ShowDoctorScheduleState
            string doctor_schedule = "doctor_schedule";
            if (db.CheckForKey(doctor_schedule))
                db.DeleteKey(doctor_schedule);

            db.SetValues(doctor_schedule,
                new KeyValuePair<string, string>("Осипова Татьяна", "10:00-17:00"),
                new KeyValuePair<string, string>("Амангельдиев Оспан", "09:00-15:00"),
                new KeyValuePair<string, string>("Виталий Ким", "15:00-19:00"));


        }

        static void Main(string[] args)
        {
            InitializeDbRedis();

            Console.OutputEncoding = Encoding.UTF8;

            var telegramBotToken = "XXX"; // your token here
            var telegramBotClient = new TelegramBotClient(telegramBotToken);
            var facade = new Telegram.TelegramBotClientFacade(telegramBotClient);

            telegramBotClient.OnMessage += Bot_OnMessage;

            var eventRepository = new EventRepository();

            eventRepository.AddScheduleToEvent(
                SpecialDiscountEventPublisher.Instance,
                TimeSpan.FromSeconds(15));

            eventRepository.RunWorker(facade);

            telegramBotClient.StartReceiving();

            Console.ReadLine();
        }

        static async void Bot_OnMessage(object sender, MessageEventArgs args)
        {
            var processingMiddleware = new ProcessingMiddlewareBuilder()
                .AddTelegramClient((ITelegramBotClient)sender)
                .AddComponent(new IncomingMessageLogger())
                //.AddComponent(new IncomingMessageThrottler(InMemoryRateLimiter.GetInstance()))
                .AddComponent(new IncomingMessageThrottler(new MemuraiDbRateLimiter(), 50))
                .AddComponent(new MessageProcessingLogic(InMemoryStateRepository.GetInstance()))
                .Build();

            processingMiddleware.ProcessRequest(args);
            
            await Task.CompletedTask;
        }
    }
}
