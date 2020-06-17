using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beisen.IncomeTax.Core.Common
{
    /// <summary>
    /// 定时更新缓存管理器（只会在取缓存时更新）
    /// </summary>
    public class UpdataTimingMananger
    {

        public static Dictionary<string, DataStringModel> CacheStringData = new Dictionary<string, DataStringModel>();

        public static Dictionary<string, DataObjectModel> CacheObjectData = new Dictionary<string, DataObjectModel>();
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="timeSpan">多久更新一次</param>
        /// <param name="cacheName">缓存名称</param>
        /// <param name="function">更新缓存的方法</param>
        /// <param name="args">更新缓存的方法参数</param>
        public static void SetUpdataStringTiming(TimeSpan timeSpan,string cacheName,Func<Args, string> function, Args args)
        {
            if (CacheStringData.ContainsKey(cacheName))
            {
                return;
            }
            else
            {
                DataStringModel dataStringModel = new DataStringModel();
                dataStringModel.DataString = function.Invoke(args);
                dataStringModel.LastRequestTime = DateTime.Now;
                dataStringModel.TimeSpan = timeSpan;
                dataStringModel.Function = function;
                dataStringModel.args = args;
                CacheStringData.Add(cacheName, dataStringModel);
            }
        }
        /// <summary>
        /// 设置缓存
        /// </summary>
        /// <param name="timeSpan">多久更新一次</param>
        /// <param name="cacheName">缓存名称</param>
        /// <param name="function">更新缓存的方法</param>
        /// <param name="args">更新缓存的方法参数</param>
        public static void SetUpdataObjectTiming(TimeSpan timeSpan, string cacheName, Func<Args, object> function, Args args)
        {
            if (CacheObjectData.ContainsKey(cacheName))
            {
                return;
            }
            else
            {
                DataObjectModel dataStringModel = new DataObjectModel();
                dataStringModel.DataObject = function.Invoke(args);
                dataStringModel.LastRequestTime = DateTime.Now;
                dataStringModel.TimeSpan = timeSpan;
                dataStringModel.Function = function;
                dataStringModel.args = args;
                CacheObjectData.Add(cacheName, dataStringModel);
            }
        }
        /// <summary>
        /// 获取缓存的值
        /// </summary>
        /// <param name="cacheName"></param>
        /// <returns></returns>
        public static string GetUpdataString(string cacheName)
        {
            if (CacheStringData.ContainsKey(cacheName))
            {
                DataStringModel model = CacheStringData[cacheName];
                if (DateTime.Now> model.LastRequestTime.Add(model.TimeSpan))
                {
                    lock (model.DataString)
                    {
                        if (DateTime.Now > model.LastRequestTime.Add(model.TimeSpan))
                        {
                            model.DataString = model.Function.Invoke(model.args);
                        }
                        else
                        {
                            return model.DataString;
                        }
                        
                    }
                    return model.DataString;
                }
                else
                {
                    return model.DataString;
                }
            }
            else
            {
                return "缓存管理器中不包含该缓存";
            }
        }
        /// <summary>
        /// 获取缓存的值
        /// </summary>
        /// <param name="cacheName"></param>
        /// <returns></returns>
        public static object GetUpdataObject(string cacheName)
        {
            if (CacheObjectData.ContainsKey(cacheName))
            {
                DataObjectModel model = CacheObjectData[cacheName];
                if (DateTime.Now > model.LastRequestTime.Add(model.TimeSpan))
                {
                    lock (model.DataObject)
                    {
                        if (DateTime.Now > model.LastRequestTime.Add(model.TimeSpan))
                        {
                            model.DataObject = model.Function.Invoke(model.args);
                        }
                        else
                        {
                            return model.DataObject;
                        }

                    }
                    return model.DataObject;
                }
                else
                {
                    return model.DataObject;
                }
            }
            else
            {
                return "缓存管理器中不包含该缓存";
            }
        }

    }
    public class DataStringModel
    {
        /// <summary>
        /// 上次请求时间
        /// </summary>
        public DateTime LastRequestTime;
        /// <summary>
        /// 缓存的字符串
        /// </summary>
        public string DataString;
        /// <summary>
        /// 缓存失效后执行的方法
        /// </summary>
        public Func<Args, string> Function;
        /// <summary>
        /// 间隔多久缓存失效
        /// </summary>
        public TimeSpan TimeSpan;
        /// <summary>
        /// 入参
        /// </summary>
        public Args args;
    }
    public class DataObjectModel
    {
        /// <summary>
        /// 上次请求时间
        /// </summary>
        public DateTime LastRequestTime;
        /// <summary>
        /// 缓存的object对象
        /// </summary>
        public object DataObject;
        /// <summary>
        /// 缓存失效后执行的方法
        /// </summary>
        public Func<Args, object> Function;
        /// <summary>
        /// 间隔多久缓存失效
        /// </summary>
        public TimeSpan TimeSpan;
        /// <summary>
        /// 入参
        /// </summary>
        public Args args;
    }
    public class Args
    {
        public int TenantId;
    }
}
