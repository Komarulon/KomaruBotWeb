using KomaruBot.WebAPI.DAL;
using KomaruBot.WebAPI.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace KomaruBot.DAL
{
    /// <summary>
    /// Provides acces to a Redis Key Value cache
    /// </summary>
    public class RedisContext
    {
        private IDatabase db
        {
            get
            {
                lock (lockObj)
                {
                    if (this._db == null)
                    {
                        this._db = this.multiplexer.GetDatabase();
                    }
                }

                return this._db;
            }
        }
        private IDatabase _db;

        private IConnectionMultiplexer multiplexer
        {
            get
            {
                if (_multiplexer == null)
                {
                    lock (lockObj)
                    {
                        if (_multiplexer == null)
                        {
                            _multiplexer = ConnectionMultiplexer.Connect(settings.Value.RedisConnectionString);
                        }
                    }
                }

                _multiplexer.PreserveAsyncOrder = false;

                return _multiplexer;
            }
        }

        private static object lockObj = new object();
        private static IConnectionMultiplexer _multiplexer = null;

        private IOptions<AppSettings> settings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="redisMultiplexer"></param>
        public RedisContext(IOptions<AppSettings> settings)
        {
            this.settings = settings;
        }

        #region Get and MGET

        /// <summary>
        /// Gets an item, given a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }
            var value = this.db.StringGet(key);

            if (IsNullValue(value))
            {
                return default(T);
            }

            return Deserialize<T>(value);
        }

        /// <summary>
        /// Gets an item, given a key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return default(T);
            }
            var value = await this.db.StringGetAsync(key);

            if (IsNullValue(value))
            {
                return default(T);
            }

            return Deserialize<T>(value);
        }

        /// <summary>
        /// Gets an item, given a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public object Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var value = this.db.StringGet(key);
            if (IsNullValue(value))
            {
                return null;
            }

            return Deserialize<object>(value);
        }

        /// <summary>
        /// Gets an item, given a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<object> GetAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            var value = await this.db.StringGetAsync(key);
            if (IsNullValue(value))
            {
                return null;
            }

            return Deserialize<object>(value);
        }

        private const int MaxGetAtOnce = 500;

        /// <summary>
        /// Performs an MGET on Redis, getting multiple values at once
        /// <para>Faster than performing GETs in sequence</para>
        /// <para>Returns only objects that are found (won't return a list containing nulls or defaults if they're missing)</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<T> Get<T>(string[] keys)
        {
            var result = new List<T>();

            if (keys == null || !keys.Any())
            {
                return result;
            }

            var done = false;
            int iteration = 0;

            do
            {
                var skipValue = (MaxGetAtOnce * iteration);

                var listSize = (keys.Length - skipValue);
                if (listSize > MaxGetAtOnce)
                {
                    listSize = MaxGetAtOnce;
                }

                if (listSize <= 0)
                {
                    done = true;
                    break;
                }

                var arg = new RedisKey[listSize];
                for (int i = 0; i < listSize; i++)
                {
                    // TODO: string.isnulloremptycheck on keys[i]
                    arg[i] = keys[skipValue + i];
                }

                var values = this.db.StringGet(arg);

                for (int i = 0, max = values.Count(); i < max; i++)
                {
                    var a = values[i];
                    if (!IsNullValue(a))
                    {
                        result.Add(Deserialize<T>(a));
                    }
                }

                iteration++;
            }
            while (!done);

            return result;
        }

        /// <summary>
        /// Performs an MGET on Redis, getting multiple values at once
        /// <para>Faster than performing GETs in sequence</para>
        /// <para>Returns only objects that are found (won't return a list containing nulls or defaults if they're missing)</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="keys"></param>
        /// <returns></returns>
        public async Task<List<T>> GetAsync<T>(string[] keys)
        {
            var result = new List<T>();

            if (keys == null || !keys.Any())
            {
                return result;
            }

            var done = false;
            int iteration = 0;

            do
            {
                var skipValue = (MaxGetAtOnce * iteration);

                var listSize = (keys.Length - skipValue);
                if (listSize > MaxGetAtOnce)
                {
                    listSize = MaxGetAtOnce;
                }

                if (listSize <= 0)
                {
                    done = true;
                    break;
                }

                var arg = new RedisKey[listSize];
                for (int i = 0; i < listSize; i++)
                {
                    // TODO: string.isnulloremptycheck on keys[i]
                    arg[i] = keys[skipValue + i];
                }

                var values = await this.db.StringGetAsync(arg);

                for (int i = 0, max = values.Count(); i < max; i++)
                {
                    var a = values[i];
                    if (!IsNullValue(a))
                    {
                        result.Add(Deserialize<T>(a));
                    }
                }

                iteration++;
            }
            while (!done);

            return result;
        }

        #endregion

        /// <summary>
        /// Deletes an object at the given key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Delete(string key) 
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }
            return this.db.KeyDelete(key);
        }

        #region Set

        /// <summary>
        /// Sets an object at the given key. It will never expire.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(string key, object value) { this._Set(key, value, null); }

        /// <summary>
        /// Sets an object at the given key, value, and expiration.
        /// </summary>
        /// <param name="toSet"></param>
        public void Set(RedisObjectValue toSet) { this._Set(toSet.key, toSet.value, toSet.expiration); }

        /// <summary>
        /// Sets an object at the given key, to be removed after the given timespan
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        public void Set(string key, object value, TimeSpan expiration) { this._Set(key, value, DateTime.Now.Add(expiration)); }

        /// <summary>
        /// Sets an object at the given key, to be removed after the given timespan
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        public void Set(string key, object value, TimeSpan? expiration) { this._Set(key, value, expiration == null ? ((DateTime?)null) : ((DateTime?)(DateTime.Now.Add(expiration.Value)))); }

        /// <summary>
        /// Sets an object at the given key, to be removed at the given datetime
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        public void Set(string key, object value, DateTime expiration) { this._Set(key, value, expiration); }

        /// <summary>
        /// Sets an object at the given key, to be removed at the given datetime
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        public void Set(string key, object value, DateTime? expiration) { this._Set(key, value, expiration); }

        private void _Set(string key, object value, DateTime? expiration)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (expiration.HasValue)
            {
                if (DateTime.Now < expiration.Value)
                {
                    var ts = expiration - DateTime.Now;
                    this.db.StringSet(key, Serialize(value), ts, When.Always);
                }
                else
                {
                    // Do nothing, you just said it already expired
                }
            }
            else
            {
                this.db.StringSet(key, Serialize(value));
            }
        }

        /// <summary>
        /// Sets multiple values at once
        /// <para>HIGHLY recommended to NOT use exiprations (leave expiration as null)</para>
        /// <para>Updates can be done significantly faster with no expiration (almost 20 times faster!)</para>
        /// </summary>
        /// <param name="values"></param>
        public void Set(List<RedisObjectValue> values)
        {
            if (values == null || !values.Any())
            {
                return;
            }
            if (values.Count == 1)
            {
                this.Set(values.First());
            }
            else
            {
                var valuesWithoutExpirations = new List<KeyValuePair<RedisKey,RedisValue>>();

                for (int i = 0, max = values.Count; i < max; i++)
                {
                    var a = values[i];
                    if (a.expiration.HasValue)
                    {
                        // MSET can't set expiration dates, so if there are expirations, we ahve to just set them manually
                        this.Set(a);
                    }
                    else
                    {
                        valuesWithoutExpirations.Add(new KeyValuePair<RedisKey, RedisValue>(a.key, Serialize(a.value)));
                    }
                }

                // Use Redis MSET for values without operations
                if (valuesWithoutExpirations.Any())
                {
                    this.db.StringSet(valuesWithoutExpirations.ToArray());
                }
            }
        }

        /// <summary>
        /// Sets an object at the given key. It will never expire.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task SetAsync(string key, object value) { await this._SetAsync(key, value, null); }

        /// <summary>
        /// Sets an object at the given key, value, and expiration.
        /// </summary>
        /// <param name="toSet"></param>
        public async Task SetAsync(RedisObjectValue toSet) { await this._SetAsync(toSet.key, toSet.value, toSet.expiration); }

        /// <summary>
        /// Sets an object at the given key, to be removed after the given timespan
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        public async Task SetAsync(string key, object value, TimeSpan expiration) { await this._SetAsync(key, value, DateTime.Now.Add(expiration)); }

        /// <summary>
        /// Sets an object at the given key, to be removed after the given timespan
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        public async Task SetAsync(string key, object value, TimeSpan? expiration) { await this._SetAsync(key, value, expiration == null ? ((DateTime?)null) : ((DateTime?)(DateTime.Now.Add(expiration.Value)))); }

        /// <summary>
        /// Sets an object at the given key, to be removed at the given datetime
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        public async Task SetAsync(string key, object value, DateTime expiration) { await this._SetAsync(key, value, expiration); }

        /// <summary>
        /// Sets an object at the given key, to be removed at the given datetime
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiration"></param>
        public async Task SetAsync(string key, object value, DateTime? expiration) { await this._SetAsync(key, value, expiration); }

        private async Task _SetAsync(string key, object value, DateTime? expiration)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (expiration.HasValue)
            {
                if (DateTime.Now < expiration.Value)
                {
                    var ts = expiration - DateTime.Now;
                    await this.db.StringSetAsync(key, Serialize(value), ts, When.Always);
                }
                else
                {
                    // Do nothing, you just said it already expired
                }
            }
            else
            {
                await this.db.StringSetAsync(key, Serialize(value));
            }
        }

        /// <summary>
        /// Sets multiple values at once
        /// <para>HIGHLY recommended to NOT use exiprations (leave expiration as null)</para>
        /// <para>Updates can be done significantly faster with no expiration (almost 20 times faster!)</para>
        /// </summary>
        /// <param name="values"></param>
        public async Task SetAsync(List<RedisObjectValue> values)
        {
            if (values == null || !values.Any())
            {
                return;
            }
            if (values.Count == 1)
            {
                await this.SetAsync(values.First());
            }
            else
            {
                var valuesWithoutExpirations = new List<KeyValuePair<RedisKey, RedisValue>>();

                for (int i = 0, max = values.Count; i < max; i++)
                {
                    var a = values[i];
                    if (a.expiration.HasValue)
                    {
                        // MSET can't set expiration dates, so if there are expirations, we ahve to just set them manually
                        await this.SetAsync(a);
                    }
                    else
                    {
                        valuesWithoutExpirations.Add(new KeyValuePair<RedisKey, RedisValue>(a.key, Serialize(a.value)));
                    }
                }

                // Use Redis MSET for values without operations
                if (valuesWithoutExpirations.Any())
                {
                    await this.db.StringSetAsync(valuesWithoutExpirations.ToArray());
                }
            }
        }

        #endregion

        #region Sorted Set Operations

        /// <summary>
        /// Sets an item in a sorted set. 
        /// <para>Updates existing items in the set if they have the same identity. </para>
        /// <para>Because of this, make sure the object set is the IDENTITY of the object and does not change over time.</para>
        /// </summary>
        /// <param name="setKey"></param>
        /// <param name="objectIdentity"></param>
        /// <param name="score"></param>
        public void SortedSet_Set(string setKey, string objectIdentity, double score) { this.SortedSet_Set(setKey, new RedisSortedSetObjectValue(objectIdentity, score)); }

        /// <summary>
        /// Sets an item in a sorted set. 
        /// <para>Updates existing items in the set if they have the same identity. </para>
        /// <para>Because of this, make sure the object set is the IDENTITY of the object and does not change over time.</para>
        /// </summary>
        /// <param name="setKey"></param>
        /// <param name="toSet"></param>
        public void SortedSet_Set(string setKey, RedisSortedSetObjectValue toSet)
        {
            this.db.SortedSetAdd(setKey, toSet.value, toSet.score);
        }

        /// <summary>
        /// Sets items in a sorted set. 
        /// <para>Updates existing items in the set if they have the same identity. </para>
        /// <para>Because of this, make sure the object set is the IDENTITY of the object and does not change over time.</para>
        /// </summary>
        /// <param name="setKey"></param>
        /// <param name="toSet"></param>
        public void SortedSet_Set(string setKey, RedisSortedSetObjectValue[] toSet)
        {
            var toSetArr = new SortedSetEntry[toSet.Length];
            for (int i = 0, max = toSet.Length; i < max; i++)
            {
                toSetArr[i] = new SortedSetEntry(toSet[i].value, toSet[i].score);
            }

            this.db.SortedSetAdd(setKey, toSetArr);
        }

        /// <summary>
        /// Gets all items in a sorted set
        /// </summary>
        /// <param name="setKey"></param>
        /// <returns></returns>
        public string[] SortedSet_GetAll(string setKey)
        {
            return this.SortedSet_GetByScore(setKey, double.NegativeInfinity, double.PositiveInfinity);
        }

        /// <summary>
        /// Gets all items in a sorted set with a score between the min and max score
        /// <para>Scores are inclusive, so if you say min = 1, max = 5, scores 1-5 are returned</para>
        /// </summary>
        /// <param name="setKey"></param>
        /// <param name="minScore"></param>
        /// <param name="maxScore"></param>
        /// <returns></returns>
        public string[] SortedSet_GetByScore(string setKey, double minScore, double maxScore)
        {
            var fromRedis = this.db.SortedSetRangeByScore(setKey, minScore, maxScore);
            return fromRedisArr(fromRedis);
        }

        /// <summary>
        /// Gets all items in a sorted set between the indices provided
        /// <para>Indices are inclusive, so if you say begin = 0, max = 9, the first 10 items are returned</para>
        /// </summary>
        /// <param name="setKey"></param>
        /// <param name="beginIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="orderAscending"></param>
        /// <returns></returns>
        public string[] SortedSet_GetByIndex(string setKey, long beginIndex, long endIndex, bool orderAscending = true)
        {
            var order = orderAscending ? Order.Ascending : Order.Descending;
            var fromRedis = this.db.SortedSetRangeByRank(setKey, beginIndex, endIndex, order);
            return fromRedisArr(fromRedis);
        }

        /// <summary>
        /// Gets all items in a sorted set
        /// </summary>
        /// <param name="setKey"></param>
        /// <returns></returns>
        public KeyValuePair<string, double>[] SortedSet_GetAll_WithScores(string setKey)
        {
            return this.SortedSet_GetByScore_WithScores(setKey, double.NegativeInfinity, double.PositiveInfinity);
        }

        /// <summary>
        /// Gets all items in a sorted set with a score between the min and max score
        /// <para>Scores are inclusive, so if you say min = 1, max = 5, scores 1-5 are returned</para>
        /// </summary>
        /// <param name="setKey"></param>
        /// <param name="minScore"></param>
        /// <param name="maxScore"></param>
        /// <returns></returns>
        public KeyValuePair<string, double>[] SortedSet_GetByScore_WithScores(string setKey, double minScore, double maxScore)
        {
            var fromRedis = this.db.SortedSetRangeByScoreWithScores(setKey, minScore, maxScore);
            return fromRedisArr(fromRedis);
        }

        /// <summary>
        /// Gets all items in a sorted set between the indices provided
        /// <para>Indices are inclusive, so if you say begin = 0, max = 9, the first 10 items are returned</para>
        /// </summary>
        /// <param name="setKey"></param>
        /// <param name="beginIndex"></param>
        /// <param name="endIndex"></param>
        /// <param name="orderAscending"></param>
        /// <returns></returns>
        public KeyValuePair<string, double>[] SortedSet_GetByIndex_WithScores(string setKey, long beginIndex, long endIndex, bool orderAscending = true)
        {
            var order = orderAscending ? Order.Ascending : Order.Descending;
            var fromRedis = this.db.SortedSetRangeByRankWithScores(setKey, beginIndex, endIndex, order);
            return fromRedisArr(fromRedis);
        }

        /// <summary>
        /// Removes an item from a sorted set without deleting the set
        /// </summary>
        /// <param name="setKey"></param>
        /// <param name="valueToDelete"></param>
        public void SortedSet_Delete(string setKey, string valueToDelete)
        {
            this.db.SortedSetRemove(setKey, valueToDelete);
        }



        #endregion

        #region Pattern Operations

        /// <summary>
        /// Gets all keys that match a * pattern
        /// <para>!!! This call is very slow. Use with caution in production !!!</para>
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public List<string> GetKeysByPattern_SLOW(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return new List<string>();
            }
            var res = new List<string>();
            var endPoints = multiplexer.GetEndPoints();
            foreach (var endPoint in endPoints)
            {
                var server = multiplexer.GetServer(endPoint);
                var keys = server.Keys(this.db.Database, pattern).ToArray();
                for (int i = 0, max = keys.Length; i < max; i++)
                {
                    var a = keys[i];
                    res.Add(a);
                }
                
            }
            return res;
        }

        /// <summary>
        /// Deletes all keys that match a * pattern
        /// <para>!!! This call is very slow. Use with caution in production !!!</para>
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns>Number of keys deleted</returns>
        public int DeleteKeysByPattern_SLOW(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return 0;
            }

            int total = 0;
            var endPoints = multiplexer.GetEndPoints();
            foreach (var endPoint in endPoints)
            {
                var server = multiplexer.GetServer(endPoint);
                var keysToDelete = server.Keys(this.db.Database, pattern).ToArray();

                int skip = 0, take = 5000;
                for (var curKeys = keysToDelete.Skip(skip).Take(take).ToArray(); curKeys.Any(); skip += take, curKeys = keysToDelete.Skip(skip).Take(take).ToArray())
                {
                    total += curKeys.Length;
                    this.db.KeyDelete(curKeys);
                }
            }
            return total;
        }

        #endregion

        #region Serialization 

        private static Newtonsoft.Json.JsonSerializer _serializer = null;
        private static JsonSerializer getSerializer()
        {

            if (_serializer == null)
            {
                _serializer = new Newtonsoft.Json.JsonSerializer();
                _serializer.Formatting = Formatting.None;
                _serializer.NullValueHandling = NullValueHandling.Ignore;
            }
            return _serializer;
        }

        private string Serialize(object o)
        {
            if (o == null)
            {
                return null;
            }

            var writer = new StringWriter();

            getSerializer().Serialize(writer, o);

            return writer.ToString();

            // Binary Serialization: 
            //var binaryFormatter = new BinaryFormatter();
            //using (var memoryStream = new MemoryStream())
            //{
            //    binaryFormatter.Serialize(memoryStream, o);
            //    byte[] objectDataAsStream = memoryStream.ToArray();
            //    return objectDataAsStream;
            //}
        }

        private T Deserialize<T>(RedisValue toDeserialize)
        {
            if (IsNullValue(toDeserialize))
            {
                return default(T);
            }

            // The original method of serialization used binary formatting. 
            // Now, we use JSON as it is a smaller format. So, first, try
            // to deserialize it in JSON

            T result;
            if (TryDeserialize(toDeserialize, out result))
            {
                return result;
            }

            // If that fails, use the old format of binary deserialization
            
            byte[] stream = toDeserialize;
            if (stream == null)
            {
                return default(T);
            }

            var binaryFormatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream(stream))
            {
                result = (T)binaryFormatter.Deserialize(memoryStream);
                return result;
            }
        }

        /// <summary>
        /// Attempts to deserialize JSON
        /// <para>Returns false if unable to deserialize</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="strInput"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private static bool TryDeserialize<T>(string strInput, out T result)
        {
            if (string.IsNullOrWhiteSpace(strInput))
            {
                result = default(T);
                return false;
            }

            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    result = JsonConvert.DeserializeObject<T>(strInput);
                    return true;
                }
                catch (JsonReaderException)
                {
                    //Exception in parsing json
                    result = default(T);
                    return false;
                }
                catch (Exception) //some other exception
                {
                    result = default(T);
                    // TODO: what to do here?
                    //Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                result = default(T);
                return false;
            }
        }

        private static bool IsNullValue(RedisValue me)
        {
            return (me == RedisValue.Null || me == RedisValue.EmptyString || !me.HasValue || me.IsNull || me.IsNullOrEmpty);
        }


        #endregion

        private string[] fromRedisArr(RedisValue[] from)
        {
            if (from == null)
            {
                return null;
            }

            var res = new string[from.Length];
            for (int i = 0, max = from.Length; i < max; i++)
            {
                res[i] = from[i];
            }

            return res;
        }

        private KeyValuePair<string, double>[] fromRedisArr(SortedSetEntry[] from)
        {
            if (from == null)
            {
                return null;
            }

            var res = new KeyValuePair<string, double>[from.Length];
            for (int i = 0, max = from.Length; i < max; i++)
            {
                res[i] = new KeyValuePair<string, double>(from[i].Element, from[i].Score);
            }

            return res;
        }
    }
}
