using System;
using System.Collections.Generic;

namespace MCConsultBot.Database.Repositories
{
    interface IRateLimiterRepository
    {
        void Increase(string key);
        int Get(string key);
        void Refresh();
    }

    /// <summary>
    /// Здесь мы используем паттерн "Одиночка/Singleton" для получения одной
    /// и той же копии объекта InMemoryRateLimiter, чтобы состояние счетчика
    /// не терялось на каждый запрос
    /// </summary>
    class InMemoryRateLimiter : IRateLimiterRepository
    {
        private InMemoryRateLimiter()
        {
            _counter = new Dictionary<string, int>();
        }
        private static InMemoryRateLimiter _instance;
        private Dictionary<string, int> _counter;

        public static InMemoryRateLimiter GetInstance()
        {
            if (_instance == null)
                _instance = new InMemoryRateLimiter();

            return _instance;
        }

        public int Get(string key)
        {
            if (!_instance._counter.ContainsKey(key))
            {
                _instance._counter[key] = 0;
            }

            return _instance._counter[key];
        }

        public void Increase(string key)
        {
            Get(key);
            _instance._counter[key]++;
        }

        public void Refresh()
        {
            _instance._counter = new Dictionary<string, int>();
        }
    }


    /// <summary>
    /// In memory Redis-like Database
    /// Now, each instance of this program is synchronized with one DB
    /// </summary>
    class MemuraiDbRateLimiter : IRateLimiterRepository, IDisposable
    {
        private readonly RedisDbManager _dbManager;
        private string _dictionaryName = "counter";

        public MemuraiDbRateLimiter()
        {
            _dbManager = RedisDbManager.Instance;
        }

        public void Dispose()
        {
            Refresh();
        }

        public int Get(string key)
        {
            var value = _dbManager.GetValue(_dictionaryName, key);

            if (string.IsNullOrEmpty(value))
            {
                _dbManager.SetValues(_dictionaryName, new KeyValuePair<string, string>(key, "0"));
            }

            return int.Parse(value);
        }

        public void Increase(string key)
        {
            _dbManager.IncrementBy(_dictionaryName, key, 1);
        }

        public void Refresh()
        {
            _dbManager.DeleteKey(_dictionaryName);
        }
    }
}
