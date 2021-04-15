using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCConsultBot.Database
{
    /// <summary>
    /// Fasade class for StackExchange.Redis API
    ///
    /// </summary>
    class RedisDbManager
    {
        private readonly ConnectionMultiplexer _multiplexer;
        private readonly IDatabase _database;

        private static readonly Lazy<RedisDbManager> _instance =
            new Lazy<RedisDbManager>(() => new RedisDbManager());

        public static RedisDbManager Instance { get { return _instance.Value; } }

        private RedisDbManager()
        {
            _multiplexer = ConnectionMultiplexer
                .Connect($"127.0.0.1:6379,resolvedns=1,abortConnect=False,password=");

            _database = _multiplexer.GetDatabase();
        }

        /// <summary>
        /// Returns if key exists
        /// </summary>
        /// <returns>true if exists, false if not</returns>
        public bool CheckForKey(string key)
        {
            return _database.KeyExists(key);
        }


        public string GetValue(string hashKey, string fieldKey)
        {
            return _database.HashGet(hashKey, fieldKey).ToString();
        }

        public Dictionary<string, string> GetValues(string hashKey)
        {
            return _database.HashGetAll(hashKey).ToStringDictionary();
        }

        public string GetValue(string listKey, long index)
        {
            return _database.ListGetByIndex(listKey, index).ToString();
        }

        public string[] GetValues(string listKey, long stopIndex)
        {
            return _database.ListRange(listKey, 0, stopIndex).ToStringArray();
        }

        public string GetValue(string strKey)
        {
            return _database.StringGet(strKey).ToString();
        }

        
        public void SetValues(string hashKey, params KeyValuePair<string, string>[] entries)
        {
            List<HashEntry> hashEntries = new List<HashEntry>();

            foreach(var entry in entries)
            {
                hashEntries.Add(
                    new HashEntry(entry.Key, entry.Value));
            }

            _database.HashSet(hashKey, hashEntries.ToArray());
        }

        public void SetValues(string listKey, params string[] values)
        {
            _database.ListRightPush(listKey, values.ToRedisValueArray());
        }


        public void IncrementBy(string hashKey, string fieldKey, long increment)
        {
            _database.HashIncrement(hashKey, fieldKey, increment);
        }


        public void DeleteKey(string key)
        {
            _database.KeyDelete(key);
        }

        public void DeleteHashValues(string hashKey, string[] values)
        {
            _database.HashDelete(hashKey, values.ToRedisValueArray());
        }
    }
}
