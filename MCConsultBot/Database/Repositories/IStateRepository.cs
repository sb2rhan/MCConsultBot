using MCConsultBot.State;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCConsultBot.Database.Repositories
{
    class InMemoryStateRepository
    {
        private InMemoryStateRepository()
        {
            _statesStorage = new Dictionary<long, CurrentStateHolder>();
        }

        private static InMemoryStateRepository _instance;
        private Dictionary<long, CurrentStateHolder> _statesStorage;

        public static InMemoryStateRepository GetInstance()
        {
            if (_instance == null)
                _instance = new InMemoryStateRepository();

            return _instance;
        }
        public CurrentStateHolder Get(long key)
        {
            if (!_instance._statesStorage.ContainsKey(key))
            {
                var initialState = new OnStartSelectState();
                _instance._statesStorage[key] = new CurrentStateHolder(initialState);
            }

            return _instance._statesStorage[key];
        }
    }
}
