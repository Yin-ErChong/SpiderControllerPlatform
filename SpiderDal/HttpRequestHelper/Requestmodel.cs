using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace SpiderDal.HttpRequestHelper
{
    public class Requestmodel
    {
        /// <summary>
        /// 请求类型 0Post 1Get
        /// </summary>
        public int RequestType { get; set; }

        public bool IsPost
        {
            get
            {
                return RequestType == 0;
            }
            set
            {
                if (value == true) RequestType = 0;
                else RequestType = 1;
            }
        }
        public bool IsGet
        {
            get
            {
                return RequestType == 1;
            }
            set
            {
                if (value == true) RequestType = 1;
                else RequestType = 0;
            }
        }


        /// <summary>
        /// 请求数据
        /// </summary>
        public object Data { get; set; }
        /// <summary>
        /// 请求的URL
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Cookie
        /// </summary>
        public string Cookie { get; set; }
        /// <summary>
        /// 头信息
        /// </summary>
        public Dictionary<string, string> DicHeaders { get; set; }
        /// <summary>
        /// 超时时间
        /// </summary>
        public int OutTime { get; set; }
        /// <summary>
        /// 请求Key
        /// </summary>
        public string RequestKey { get; set; }

    }
    public class PaResponse
    {
        /// <summary>
        /// 返回的结果
        /// </summary>
        public string Result { get; set; }

        /// <summary>
        /// 返回的日志信息
        /// </summary>
        public string LogInfo { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSuccess { get; set; }
        public int ErrCode { get; set; }

        public string ErrMsg { get; set; }
        public HttpResponseMessage HttpResponseMessage { get; set; }
    }
}
