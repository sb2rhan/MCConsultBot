using MCConsultBot.Telegram;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCConsultBot.State.ShowBranchesStates
{
    class ShowBranchAddressesInCityState : ICurrentState
    {
        private string _selectedCity;
        public ShowBranchAddressesInCityState(string selectedCity)
        {
            _selectedCity = selectedCity;
        }

        private IEnumerable<string> GetAddressesByCity(string city)
        {
            var map = new Dictionary<string, IEnumerable<string>>();
            map["Алматы"] = new string[] { "Жарокова 176", "Абая 98" };
            map["Астана"] = new string[] { "Бокейхана 21" };
            map["Караганда"] = new string[] { "Абая 11" };

            return map[city];
        }


        public void PrerenderDefaultOutput(
            long chatId, 
            TelegramBotClientFacade telegramBotClientFacade)
        {
            telegramBotClientFacade.SendButtonMessageToChat(chatId, GetAddressesByCity(_selectedCity));
        }

        public void ProcessInput(
            long chatId, 
            string input, 
            TelegramBotClientFacade telegramBotClientFacade, 
            CurrentStateHolder stateHolder)
        {
            telegramBotClientFacade.SendMapMessageToChat(chatId);

            var initialState = new OnStartSelectState();
            initialState.PrerenderDefaultOutput(chatId, telegramBotClientFacade);
            stateHolder.SetNextState(initialState);
        }
    }
}
