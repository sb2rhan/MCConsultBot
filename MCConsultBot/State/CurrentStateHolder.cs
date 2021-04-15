using MCConsultBot.Telegram;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCConsultBot.State
{
    class CurrentStateHolder
    {
        private ICurrentState _currentState;

        public void ProcessInput(long chatId,
            string input,
            TelegramBotClientFacade telegramBotClientFacade)
        {
            _currentState
                .ProcessInput(chatId, input, telegramBotClientFacade, this);
        }

        public void SetNextState(ICurrentState nextState)
        {
            _currentState = nextState;
        }

        public CurrentStateHolder(ICurrentState initalState)
        {
            _currentState = initalState;
        }
    }
}
