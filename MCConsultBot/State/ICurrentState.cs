using MCConsultBot.Database;
using MCConsultBot.State.ShowBranchesStates;
using MCConsultBot.Telegram;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCConsultBot.State
{
    interface ICurrentState
    {
        /// <summary>
        /// Пользователь всегда присылает некий ввод. В зависимости от нашего
        /// состояния, мы по разному будем обрабатывать этот ввод.
        /// </summary>
        void ProcessInput(
            long chatId,
            string input,
            TelegramBotClientFacade telegramBotClientFacade,
            CurrentStateHolder stateHolder);

        void PrerenderDefaultOutput(
            long chatId,
            TelegramBotClientFacade telegramBotClientFacade);
    }


    /// <summary>
    /// Начальное окно выбора - список возможных операции
    /// </summary>
    class OnStartSelectState : ICurrentState
    {
        public void PrerenderDefaultOutput(
            long chatId,
            TelegramBotClientFacade telegramBotClientFacade)
        {
            telegramBotClientFacade
                    .SendButtonMessageToChat(chatId, GetButtons());
        }

        public void ProcessInput(
            long chatId,
            string input,
            TelegramBotClientFacade telegramBotClientFacade,
            CurrentStateHolder stateHolder)
        {
            var selectedButton = GetButtons()
                .FirstOrDefault(p => input
                    .Equals(p, StringComparison.InvariantCultureIgnoreCase));

            if (selectedButton == null)
            {
                telegramBotClientFacade
                    .SendButtonMessageToChat(chatId, GetButtons());
            }
            else
            {
                var nextState = ButtonToNextStateMapper(selectedButton);
                nextState.PrerenderDefaultOutput(chatId, telegramBotClientFacade);
                stateHolder.SetNextState(nextState);
            }
        }

        private ICurrentState ButtonToNextStateMapper(string button)
        {
            return button switch
            {
                "Показать расписание врача" => new ShowDoctorScheduleState(),
                "Показать кабинет врача" => new ShowDoctorRoomState(),
                "Показать филиалы медцентра" => new ShowBrachesStartState(),
                "Подписаться на акции и скидки" => new SubscribeToDiscountsState(),
                _ => new OnStartSelectState(),
            };
        }

        private IEnumerable<string> GetButtons()
        {
            var manager = RedisDbManager.Instance;

            return manager.GetValues("onstart_buttons", 10);
        }
    }

    class ShowDoctorRoomState : ICurrentState
    {
        public void PrerenderDefaultOutput(
            long chatId,
            TelegramBotClientFacade telegramBotClientFacade)
        {
            telegramBotClientFacade
                    .SendTextMessageToChat(chatId, GetRooms());
        }

        public void ProcessInput(
            long chatId, 
            string input,
            TelegramBotClientFacade telegramBotClientFacade,
            CurrentStateHolder stateHolder)
        {
            var nextState = new OnStartSelectState();

            nextState.ProcessInput(chatId, input, telegramBotClientFacade, stateHolder);

            stateHolder.SetNextState(nextState);
        }

        private string GetRooms()
        {
            var manager = RedisDbManager.Instance;

            var dictionary = manager.GetValues("doctor_room");

            string result = "";

            foreach (var kv in dictionary)
            {
                result += $"{kv.Key} - {kv.Value}{Environment.NewLine}";
            }

            return result;
        }
    }

    /// <summary>
    /// Показать расписание врача
    /// </summary>
    class ShowDoctorScheduleState : ICurrentState
    {
        public void ProcessInput(long chatId,
            string input,
            TelegramBotClientFacade telegramBotClientFacade,
            CurrentStateHolder stateHolder)
        {
            var doctorName = input;
            var schedule = GetSchedule(doctorName);

            telegramBotClientFacade.SendTextMessageToChat(chatId, schedule);

            var nextState = new OnStartSelectState();
            nextState.PrerenderDefaultOutput(chatId, telegramBotClientFacade);
            stateHolder.SetNextState(nextState);
        }

        public void PrerenderDefaultOutput(
            long chatId,
            TelegramBotClientFacade telegramBotClientFacade)
        {
            telegramBotClientFacade.SendButtonMessageToChat(chatId, GetDoctorsList());
        }

        private string GetSchedule(string doctorName)
        {
            var manager = RedisDbManager.Instance;

            var schedule = manager.GetValue("doctor_schedule", doctorName);

            return schedule != string.Empty ? schedule : "Не найден врач!";
        }

        private IEnumerable<string> GetDoctorsList()
        {
            var manager = RedisDbManager.Instance;

            return manager.GetValues("doctors", -1);
        }
    }
}
