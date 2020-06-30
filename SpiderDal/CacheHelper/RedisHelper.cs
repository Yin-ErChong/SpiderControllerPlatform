using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Jinher.AMPX.Stbx.Common.Cache
{
    public class RedisHelper
    {
        private readonly ConnectionMultiplexer connectionMultiplexer;
        private string PrefixKey;
        private IDatabase db;

        public static RedisHelper Instance { get { return RedisInstance(); } }
        public static RedisHelper RedisInstance(int dbNum = 0)
        {
            return new RedisHelper(dbNum);
        }
        #region 构造函数

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbNum"></param>
        public RedisHelper(int dbNum = 0) : this(dbNum, "")
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbNum"></param>
        /// <param name="connectionString"></param>
        public RedisHelper(int dbNum, string connectionString)
        {
            connectionMultiplexer = string.IsNullOrWhiteSpace(connectionString) ? RedisConnectionHelper.Instance : RedisConnectionHelper.GetConnectionMultiplexer(connectionString);
            db = connectionMultiplexer.GetDatabase(dbNum);
        }




        #endregion

        #region String 操作
        /// <summary>
        /// 保存单个key value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool Set(string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddPrefixKey(key);
            return db.StringSet(key, value, expiry);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <param name="when"></param>
        /// <returns></returns>
        public bool Set(string key, string value, TimeSpan? expiry = default(TimeSpan?), When when = When.Always)
        {
            key = AddPrefixKey(key);
            return db.StringSet(key, value, expiry, when);
        }

        /// <summary>
        /// 异步保存单个key value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddPrefixKey(key);
            return await db.StringSetAsync(key, value, expiry);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> SetAsync(string key, string value, TimeSpan? expiry = default(TimeSpan?), When when = When.Always)
        {
            key = AddPrefixKey(key);
            return await db.StringSetAsync(key, value, expiry, when);
        }

        /// <summary>
        /// 保存多个key value
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public bool Set(List<KeyValuePair<RedisKey, RedisValue>> keyValues)
        {
            List<KeyValuePair<RedisKey, RedisValue>> pairs = keyValues.Select(obj => new KeyValuePair<RedisKey, RedisValue>(AddPrefixKey(obj.Key), obj.Value)).ToList();
            return db.StringSet(pairs.ToArray());
        }

        /// <summary>
        /// 异步保存多个key value
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        public async Task<bool> SetAsync(List<KeyValuePair<RedisKey, RedisValue>> keyValues)
        {
            List<KeyValuePair<RedisKey, RedisValue>> pairs = keyValues.Select(obj => new KeyValuePair<RedisKey, RedisValue>(AddPrefixKey(obj.Key), obj.Value)).ToList();
            return await db.StringSetAsync(pairs.ToArray());
        }

        /// <summary>
        /// 保存key对应的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool Set<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddPrefixKey(key);
            byte[] buffer = ObjectToBytes(obj);
            if (buffer == null || buffer.Length == 0)
            {
                return false;
            }
            return db.StringSet(key, buffer, expiry);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool Set<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?), When when = When.Always)
        {
            key = AddPrefixKey(key);
            byte[] buffer = ObjectToBytes(obj);
            if (buffer == null || buffer.Length == 0)
            {
                return false;
            }
            return db.StringSet(key, buffer, expiry, when);
        }

        /// <summary>
        /// 异步 保存key对应的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> SetAsync<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddPrefixKey(key);
            byte[] buffer = ObjectToBytes(obj);
            if (buffer == null || buffer.Length == 0)
            {
                return false;
            }
            return await db.StringSetAsync(key, buffer, expiry);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> SetAsync<T>(string key, T obj, TimeSpan? expiry = default(TimeSpan?), When when = When.Always)
        {
            key = AddPrefixKey(key);
            byte[] buffer = ObjectToBytes(obj);
            if (buffer == null || buffer.Length == 0)
            {
                return false;
            }
            return await db.StringSetAsync(key, buffer, expiry, when);
        }

        /// <summary>
        /// 获取key的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string Get(string key)
        {
            key = AddPrefixKey(key);
            return db.StringGet(key);
        }

        /// <summary>
        /// 异步获取key的值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<string> GetAsync(string key)
        {
            key = AddPrefixKey(key);
            return await db.StringGetAsync(key);
        }

        /// <summary>
        /// 获取多个key
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public RedisValue[] Get(List<string> list)
        {
            List<string> keys = list.Select(AddPrefixKey).ToList();
            return db.StringGet(ConvertRedisKeys(keys));
        }

        /// <summary>
        /// 异步获取多个key
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<RedisValue[]> GetAsync(List<string> list)
        {
            List<string> keys = list.Select(AddPrefixKey).ToList();
            return await db.StringGetAsync(ConvertRedisKeys(keys));
        }

        #region Bit

        /// <summary>
        /// 计算字符串中设置的比特数(总体计数)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public long StringBitCount(RedisKey key, long start = 0, long end = -1)
        {
            key = AddPrefixKey(key);
            return db.StringBitCount(key, start, end);
        }

        /// <summary>
        /// 异步计算字符串中设置的比特数(总体计数)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<long> StringBitCountAsync(RedisKey key, long start = 0, long end = -1)
        {
            key = AddPrefixKey(key);
            return await db.StringBitCountAsync(key, start, end);
        }

        /// <summary>
        /// 在多个键之间执行按位操作(包含字符串值)
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="destination"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey[] keys)
        {
            return db.StringBitOperation(operation, destination, keys);
        }

        /// <summary>
        /// 在多个键之间执行按位操作(包含字符串值)
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="destination"></param>
        /// <param name="keys"></param>
        /// <returns></returns>
        public async Task<long> StringBitOperationAsync(Bitwise operation, RedisKey destination, RedisKey[] keys)
        {
            return await db.StringBitOperationAsync(operation, destination, keys);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="destination"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public long StringBitOperation(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second)
        {
            return db.StringBitOperation(operation, destination, first, second);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="destination"></param>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public async Task<long> StringBitOperationAsync(Bitwise operation, RedisKey destination, RedisKey first, RedisKey second)
        {
            return await db.StringBitOperationAsync(operation, destination, first, second);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="bit"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public long StringBitPosition(RedisKey key, bool bit, long start = 0, long end = -1)
        {
            return db.StringBitPosition(key, bit, start, end);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="bit"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public async Task<long> StringBitPositionAsync(RedisKey key, bool bit, long start = 0, long end = -1)
        {
            return await db.StringBitPositionAsync(key, bit, start, end);
        }

        #endregion

        #endregion

        #region Increment/Decrement 操作
        /// <summary>
        /// 为数字增长指定的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public double StringIncrement(string key, double val = 1)
        {
            key = AddPrefixKey(key);
            return db.StringIncrement(key, val);
        }

        /// <summary>
        /// 为数字增长指定的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public void StringIncrement(string key, TimeSpan? expiry, double val = 1)
        {
            key = AddPrefixKey(key);

            db.StringIncrement(key, val);

            if (!KeyExpire(key, expiry))
            {
                KeyExpire(key, expiry);
            }
        }

        /// <summary>
        /// 异步为数字增长指定的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public async Task<double> StringIncrementAsync(string key, double val = 1)
        {
            key = AddPrefixKey(key);
            return await db.StringIncrementAsync(key, val);
        }

        /// <summary>
        /// 异步为数字增长指定的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public async Task StringIncrementAsync(string key, TimeSpan? expiry, double val = 1)
        {
            key = AddPrefixKey(key);

            await db.StringIncrementAsync(key, val);

            if (!await KeyExpireAsync(key, expiry))
            {
                await KeyExpireAsync(key, expiry);
            }
        }

        /// <summary>
        /// 减少指定的数值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public double StringDecrement(string key, double val = 1)
        {
            key = AddPrefixKey(key);
            return db.StringDecrement(key, val);
        }

        /// <summary>
        /// 减少指定的数值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public void StringDecrement(string key, TimeSpan? expiry, double val = 1)
        {
            key = AddPrefixKey(key);

            db.StringDecrement(key, val);

            if (!KeyExpire(key, expiry))
            {
                KeyExpire(key, expiry);
            }
        }

        /// <summary>
        /// 异步减少指定的数值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public async Task<double> StringDecrmentAsync(string key, double val = 1)
        {
            key = AddPrefixKey(key);
            return await db.StringDecrementAsync(key, val);
        }

        /// <summary>
        /// 异步减少指定的数值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="expiry"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public async Task StringDecrmentAsync(string key, TimeSpan? expiry, double val = 1)
        {
            key = AddPrefixKey(key);

            await db.StringDecrementAsync(key, val);

            if (!await KeyExpireAsync(key, expiry))
            {
                await KeyExpireAsync(key, expiry);
            }
        }

        /// <summary>
        /// hash值增加指定的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public double HashIncrement(string key, string dataKey, double val = 1)
        {
            key = AddPrefixKey(key);
            return db.HashIncrement(key, dataKey, val);
        }

        /// <summary>
        /// 异步hash值增加指定的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public async Task<double> HashIncrementAsync(string key, string dataKey, double val = 1)
        {
            key = AddPrefixKey(key);
            return await db.HashIncrementAsync(key, dataKey, val);
        }

        /// <summary>
        /// hash值增加指定的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public double HashDecrement(string key, string dataKey, double val = 1)
        {
            key = AddPrefixKey(key);
            return db.HashDecrement(key, dataKey, val);
        }

        /// <summary>
        /// 异步hash值增加指定的值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public async Task<double> HashDecrementAsync(string key, string dataKey, double val = 1)
        {
            key = AddPrefixKey(key);
            return await db.HashDecrementAsync(key, dataKey, val);
        }

        #endregion

        #region Hash 操作
        /// <summary>
        /// 检查某个数据是否已经被缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashExists(string key, string dataKey)
        {
            key = AddPrefixKey(key);
            return db.HashExists(key, dataKey);
        }

        /// <summary>
        /// 异步检查某个数据是否已经被缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<bool> HashExistsAsync(string key, string dataKey)
        {
            key = AddPrefixKey(key);
            return await db.HashExistsAsync(key, dataKey);
        }

        /// <summary>
        /// 存储数据到Hash表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool HashSet<T>(string key, string dataKey, T t)
        {
            key = AddPrefixKey(key);
            byte[] buffer = ObjectToBytes(t);
            if (buffer == null || buffer.Length == 0)
            {
                return false;
            }
            return db.HashSet(key, dataKey, buffer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool HashSet<T>(string key, string dataKey, T t, When when = When.Always)
        {
            key = AddPrefixKey(key);
            byte[] buffer = ObjectToBytes(t);
            if (buffer == null || buffer.Length == 0)
            {
                return false;
            }
            return db.HashSet(key, dataKey, buffer, when);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task<bool> HashSetAsync<T>(string key, string dataKey, T t)
        {
            key = AddPrefixKey(key);
            byte[] buffer = ObjectToBytes(t);
            if (buffer == null || buffer.Length == 0)
            {
                return false;
            }
            return await db.HashSetAsync(key, dataKey, buffer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task<bool> HashSetAsync<T>(string key, string dataKey, T t, When when = When.Always)
        {
            key = AddPrefixKey(key);
            byte[] buffer = ObjectToBytes(t);
            if (buffer == null || buffer.Length == 0)
            {
                return false;
            }
            return await db.HashSetAsync(key, dataKey, buffer, when);
        }

        /// <summary>
        /// 删除hash中的某个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public bool HashDelete(string key, string dataKey)
        {
            key = AddPrefixKey(key);
            return db.HashDelete(key, dataKey);
        }

        /// <summary>
        /// 异步删除hash中的某个值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<bool> HashDeleteAsync(string key, string dataKey)
        {
            key = AddPrefixKey(key);
            return await db.HashDeleteAsync(key, dataKey);
        }

        /// <summary>
        /// 从hash获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public T HashGet<T>(string key, string dataKey)
        {
            key = AddPrefixKey(key);
            var value = db.HashGet(key, dataKey);
            return ConvertObj<T>(value);
        }

        /// <summary>
        /// 异步从hash获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="dataKey"></param>
        /// <returns></returns>
        public async Task<T> HashGetAsync<T>(string key, string dataKey)
        {
            key = AddPrefixKey(key);
            var value = await db.HashGetAsync(key, dataKey);
            return ConvertObj<T>(value);
        }

        /// <summary>
        /// 获取HashKey所有key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> HashKeys<T>(string key)
        {
            key = AddPrefixKey(key);
            RedisValue[] values = db.HashKeys(key);
            return ConvertList<T>(values);
        }

        /// <summary>
        /// 异步获取HashKey所有key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> HashKeysAsync<T>(string key)
        {
            key = AddPrefixKey(key);
            RedisValue[] values = await db.HashKeysAsync(key);
            return ConvertList<T>(values);
        }

        #endregion

        #region List 操作

        /// <summary>
        /// 移除指定的list的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public long ListRemove<T>(string key, T value)
        {
            key = AddPrefixKey(key);
            return db.ListRemove(key, ObjectToBytes(value));
        }

        /// <summary>
        /// 异步移除指定的list的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> ListRemoveAsync<T>(string key, T value)
        {
            key = AddPrefixKey(key);
            return await db.ListRemoveAsync(key, ObjectToBytes(value));
        }

        /// <summary>
        /// 获取指定key的List
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> ListRange<T>(string key)
        {
            key = AddPrefixKey(key);
            var values = db.ListRange(key);
            return ConvertList<T>(values);
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public long ListRightPush<T>(string key, T value)
        {
            key = AddPrefixKey(key);
            return db.ListRightPush(key, ObjectToBytes(value));
        }

        /// <summary>
        /// 异步入队
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> ListRightPushAsync<T>(string key, T value)
        {
            key = AddPrefixKey(key);
            return await db.ListRightPushAsync(key, ObjectToBytes(value));
        }

        /// <summary>
        /// 出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListRightPop<T>(string key)
        {
            key = AddPrefixKey(key);
            var value = db.ListRightPop(key);
            return ConvertObj<T>(value);
        }

        /// <summary>
        /// 异步出队
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> ListRightPopAsync<T>(string key)
        {
            key = AddPrefixKey(key);
            var value = await db.ListRightPopAsync(key);
            return ConvertObj<T>(value);
        }

        /// <summary>
        /// 入栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public long ListLeftPush<T>(string key, T value)
        {
            key = AddPrefixKey(key);
            return db.ListLeftPush(key, ObjectToBytes(value));
        }

        /// <summary>
        /// 异步入栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<long> ListLeftPushAsync<T>(string key, T value)
        {
            key = AddPrefixKey(key);
            return await db.ListLeftPushAsync(key, ObjectToBytes(value));
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public T ListLeftPop<T>(string key)
        {
            key = AddPrefixKey(key);
            var value = db.ListLeftPop(key);
            return ConvertObj<T>(value);
        }

        /// <summary>
        /// 出栈
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> ListLeftPopAsync<T>(string key)
        {
            key = AddPrefixKey(key);
            var value = await db.ListLeftPopAsync(key);
            return ConvertObj<T>(value);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long ListLength(string key)
        {
            key = AddPrefixKey(key);
            return db.ListLength(key);
        }

        /// <summary>
        /// 异步获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> ListLengthAsync(string key)
        {
            key = AddPrefixKey(key);
            return await db.ListLengthAsync(key);
        }

        #endregion

        #region Set 操作

        /// <summary>
        /// 获取集合元素 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> SetMembers<T>(string key)
        {
            key = AddPrefixKey(key);
            var values = db.SetMembers(key);
            return ConvertList<T>(values);
        }

        /// <summary>
        /// 异步获取集合元素 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> SetMembersAsync<T>(string key)
        {
            key = AddPrefixKey(key);
            var values = await db.SetMembersAsync(key);
            return ConvertList<T>(values);
        }

        /// <summary>
        /// 保存set值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetAdd<T>(string key, T value)
        {
            key = AddPrefixKey(key);
            return db.SetAdd(key, ObjectToBytes(value));
        }

        /// <summary>
        /// 异步保存Set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task<bool> SetAddAsync<T>(string key, T value)
        {
            key = AddPrefixKey(key);
            return await db.SetAddAsync(key, ObjectToBytes(value));
        }

        /// <summary>
        /// 判断set中是否存在某个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool SetContains<T>(string key, T t)
        {
            key = AddPrefixKey(key);
            return db.SetContains(key, ObjectToBytes(t));
        }

        /// <summary>
        /// 异步判断set中是否存在某个元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task<bool> SetContainsAsync<T>(string key, T t)
        {
            key = AddPrefixKey(key);
            return await db.SetContainsAsync(key, ObjectToBytes(t));
        }

        /// <summary>
        /// 获取set中元素个数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SetLength(string key)
        {
            key = AddPrefixKey(key);
            return db.SetLength(key);
        }

        /// <summary>
        /// 异步获取set中元素个数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> SetLengthAsync(string key)
        {
            key = AddPrefixKey(key);
            return await db.SetLengthAsync(key);
        }

        /// <summary>
        /// 移除set中的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool SetRemove<T>(string key, T t)
        {
            key = AddPrefixKey(key);
            return db.SetRemove(key, ObjectToBytes(t));
        }

        /// <summary>
        /// 异步移除set中的元素
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public async Task<bool> SetRemoveAsync<T>(string key, T t)
        {
            key = AddPrefixKey(key);
            return await db.SetRemoveAsync(key, ObjectToBytes(t));
        }

        #endregion

        #region ZSet 操作
        /// <summary>
        /// 添加ZSet
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        public bool SortedSetAdd<T>(string key, T value, double score)
        {
            key = AddPrefixKey(key);
            return db.SortedSetAdd(key, ObjectToBytes(value), score);
        }

        /// <summary>
        /// 异步添加ZSet
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="score"></param>
        public async Task<bool> SortedSetAddAsync<T>(string key, T value, double score)
        {
            key = AddPrefixKey(key);
            return await db.SortedSetAddAsync(key, ObjectToBytes(value), score);
        }

        /// <summary>
        /// 删除ZSet
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public bool SortedSetRemove<T>(string key, T value)
        {
            key = AddPrefixKey(key);
            return db.SortedSetRemove(key, ObjectToBytes(value));
        }

        /// <summary>
        /// 异步删除ZSet
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async Task<bool> SortedSetRemoveAsync<T>(string key, T value)
        {
            key = AddPrefixKey(key);
            return await db.SortedSetRemoveAsync(key, ObjectToBytes(value));
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<T> SortedSetRangeByRank<T>(string key)
        {
            key = AddPrefixKey(key);
            var values = db.SortedSetRangeByRank(key);
            return ConvertList<T>(values);
        }

        /// <summary>
        /// 获取区间数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public List<T> SortedSetRangeByRank<T>(string key, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            key = AddPrefixKey(key);
            var values = db.SortedSetRangeByRank(key, start, stop, order);
            return ConvertList<T>(values);
        }

        /// <summary>
        /// 获取全部
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<List<T>> SortedSetRangeByRankAsync<T>(string key)
        {
            key = AddPrefixKey(key);
            var values = await db.SortedSetRangeByRankAsync(key);
            return ConvertList<T>(values);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<List<T>> SortedSetRangeByRankAsync<T>(string key, long start = 0, long stop = -1, Order order = Order.Ascending)
        {
            key = AddPrefixKey(key);
            var values = await db.SortedSetRangeByRankAsync(key, start, stop, order);
            return ConvertList<T>(values);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public long SortedSetLength(string key)
        {
            key = AddPrefixKey(key);
            return db.SortedSetLength(key);
        }

        /// <summary>
        /// 获取集合中的数量
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> SortedSetLengthAsync(string key)
        {
            key = AddPrefixKey(key);
            return await db.SortedSetLengthAsync(key);
        }
        #endregion

        #region Key 操作

        /// <summary>
        /// 删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public bool KeyDelete(string key)
        {
            key = AddPrefixKey(key);
            return db.KeyDelete(key);
        }

        /// <summary>
        /// 异步删除单个key
        /// </summary>
        /// <param name="key">redis key</param>
        /// <returns>是否删除成功</returns>
        public async Task<bool> KeyDeleteAsync(string key)
        {
            key = AddPrefixKey(key);
            return await db.KeyDeleteAsync(key);
        }

        /// <summary>
        /// 删除多个key
        /// </summary>
        /// <param name="keys">rediskey</param>
        /// <returns>成功删除的个数</returns>
        public long KeyDelete(List<string> keys)
        {
            List<string> newKeys = keys.Select(AddPrefixKey).ToList();
            return db.KeyDelete(ConvertRedisKeys(newKeys));
        }

        /// <summary>
        /// 异步删除多个key
        /// </summary>
        /// <param name="keys">rediskey</param>
        /// <returns>成功删除的个数</returns>
        public async Task<long> KeyDeleteAsync(List<string> keys)
        {
            List<string> newKeys = keys.Select(AddPrefixKey).ToList();
            return await db.KeyDeleteAsync(ConvertRedisKeys(newKeys));
        }

        /// <summary>
        /// 判断key是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool KeyExists(string key)
        {
            key = AddPrefixKey(key);
            return db.KeyExists(key);
        }

        /// <summary>
        /// 异步判断key是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> KeyExistsAsync(string key)
        {
            key = AddPrefixKey(key);
            return await db.KeyExistsAsync(key);
        }

        /// <summary>
        /// 重新命名key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public bool KeyRename(string key, string newKey)
        {
            key = AddPrefixKey(key);
            return db.KeyRename(key, newKey);
        }


        /// <summary>
        /// 异步重新命名key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="newKey"></param>
        /// <returns></returns>
        public async Task<bool> KeyRenameAsync(string key, string newKey)
        {
            key = AddPrefixKey(key);
            return await db.KeyRenameAsync(key, newKey);
        }

        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="key">redis key</param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public bool KeyExpire(string key, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddPrefixKey(key);
            return db.KeyExpire(key, expiry);
        }

        /// <summary>
        /// 设置Key的时间
        /// </summary>
        /// <param name="key">redis key</param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<bool> KeyExpireAsync(string key, TimeSpan? expiry = default(TimeSpan?))
        {
            key = AddPrefixKey(key);
            return await db.KeyExpireAsync(key, expiry);
        }

        #endregion

        #region 发布订阅
        /// <summary>
        /// Redis发布订阅  订阅
        /// </summary>
        /// <param name="subChannel"></param>
        /// <param name="handler"></param>
        public void Subscribe(string subChannel, Action<RedisChannel, RedisValue> handler = null)
        {
            ISubscriber sub = connectionMultiplexer.GetSubscriber();
            sub.Subscribe(subChannel, (channel, message) =>
            {
                if (handler == null)
                {
                    Console.WriteLine(subChannel + " 订阅收到消息：" + message);
                }
                else
                {
                    handler(channel, message);
                }
            });
        }

        /// <summary>
        /// Redis发布订阅  发布
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public long Publish<T>(string channel, T msg)
        {
            ISubscriber sub = connectionMultiplexer.GetSubscriber();
            return sub.Publish(channel, ObjectToBytes(msg));
        }

        /// <summary>
        /// Redis发布订阅  取消订阅
        /// </summary>
        /// <param name="channel"></param>
        public void Unsubscribe(string channel)
        {
            ISubscriber sub = connectionMultiplexer.GetSubscriber();
            sub.Unsubscribe(channel);
        }

        /// <summary>
        /// Redis发布订阅  取消全部订阅
        /// </summary>
        public void UnsubscribeAll()
        {
            ISubscriber sub = connectionMultiplexer.GetSubscriber();
            sub.UnsubscribeAll();
        }

        #endregion

        #region 开启事务
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ITransaction CreateTransaction()
        {
            return db.CreateTransaction();
        }
        #endregion

        #region 其他方法

        /// <summary>
        /// 返回批量Batch
        /// </summary>
        /// <returns></returns>
        public IBatch CreateBatch()
        {
            return db.CreateBatch();
        }

        #region SetPrefixKey 设置前缀key
        /// <summary>
        /// 设置前缀key
        /// </summary>
        /// <param name="key"></param>
        public void SetPrefixKey(string key)
        {
            PrefixKey = key;
        }
        #endregion

        #region AddPrefixKey 添加前缀key
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string AddPrefixKey(string key)
        {
            var prefixKey = PrefixKey ?? RedisConnectionHelper.RedisPrefixKey;
            return prefixKey + key;
        }
        #endregion

        #region 对象和二进制互转
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private byte[] ObjectToBytes(object obj)
        {
            byte[] buffer = null;
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                buffer = stream.ToArray();
            }
            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        private object BytesToObject(byte[] buffer)
        {
            object obj = null;
            using (MemoryStream stream = new MemoryStream(buffer))
            {
                IFormatter formatter = new BinaryFormatter();
                obj = formatter.Deserialize(stream);
            }
            return obj;
        }
        #endregion

        #region ConvertToJson
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        private string ConvertToJson<T>(T value)
        {
            string result = value is string ? value.ToString() : JsonConvert.SerializeObject(value);
            return result;
        }
        #endregion

        #region list转RedisKey[]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="redisKeys"></param>
        /// <returns></returns>
        private RedisKey[] ConvertRedisKeys(List<string> redisKeys)
        {
            return redisKeys.Select(redisKey => (RedisKey)redisKey).ToArray();
        }
        #endregion

        #region ConvertObj 
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        private T ConvertObj<T>(RedisValue value)
        {
            if (typeof(T).Name.Equals(typeof(string).Name))
            {
                return JsonConvert.DeserializeObject<T>($"'{value}'");
            }
            return JsonConvert.DeserializeObject<T>(value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <returns></returns>
        private List<T> ConvertList<T>(RedisValue[] values)
        {
            List<T> result = new List<T>();

            foreach (var item in values)
            {
                var model = ConvertObj<T>(item);
                result.Add(model);
            }

            return result;
        }

        #endregion

        #endregion
    }
}
