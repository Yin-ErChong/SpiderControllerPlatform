using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SpiderDal.HttpRequestHelper
{
    class HttpRequestService
    {
        public static Response Post(object data, string url)
        {
            Requestmodel requestModel = new Requestmodel();
            requestModel.IsPost = true;
            requestModel.Url = url;
            requestModel.Data = data;
            return AskData(requestModel);
        }
        public static Response Get(string url)
        {
            Requestmodel requestModel = new Requestmodel();
            requestModel.IsPost = true;
            requestModel.Url = url;
            return AskData(requestModel);
        }
        public static Response AskData(Requestmodel model)
        {
            Response _paResponse = new Response();
            _paResponse.ErrCode = 0;
            _paResponse.ErrMsg = "成功";

            try
            {
                using (HttpClient client = new HttpClient(new HttpClientHandler()))
                {
                    //client.GetHostConfiguration().setProxy("192.168.101.1", 5608);

                    client.BaseAddress = new Uri(model.Url);
                    if (model.OutTime > 0)
                    {
                        client.Timeout = new TimeSpan(0, 0, 0, 0, model.OutTime * 1000);
                    }
                    else
                    {
                        client.Timeout = new TimeSpan(0, 0, 0, 0, 30000);
                    }

                    MediaTypeHeaderValue typeHeader = null;

                    #region 添加头信息
                    if (model.DicHeaders != null && model.DicHeaders.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> item in model.DicHeaders)
                        {
                            //添加头信息
                            if (item.Key != null && item.Value != null)
                            {
                                if (item.Key == "Content-Type")
                                {
                                    if (item.Value == "application/json;charset=utf-8")
                                    {
                                        typeHeader = new MediaTypeHeaderValue("application/json");
                                        typeHeader.CharSet = "utf-8";
                                    }
                                    else if (item.Value == "application/x-www-form-urlencoded; charset=UTF-8")
                                    {
                                        typeHeader = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
                                        typeHeader.CharSet = "UTF-8";
                                    }
                                    else
                                    {
                                        if (item.Value.Contains(";"))
                                        {
                                            var temp = item.Value.Split(';');
                                            typeHeader = new MediaTypeHeaderValue(temp[0]);
                                            typeHeader.CharSet = temp[1].Split('=')[1];
                                        }
                                        else
                                        {
                                            typeHeader = new MediaTypeHeaderValue(item.Value);
                                        }
                                    }
                                }
                                else if (item.Key != "Content-Length")
                                {
                                    try
                                    {
                                        client.DefaultRequestHeaders.Add(item.Key, item.Value);
                                    }
                                    catch (Exception ee)
                                    {

                                    }
                                }
                            }
                        }
                    }
                    #endregion

                    #region HttpContent
                    HttpContent content = null;
                    if (model.IsPost && model.Data != null)
                    {
                        if (model.Data is string)
                        {
                            content = new StringContent(model.Data as string);
                        }
                        else
                        {
                            content = new StringContent(JsonConvert.SerializeObject(model.Data));
                        }

                        if (typeHeader == null)
                        {
                            typeHeader = new MediaTypeHeaderValue("application/json");
                        }

                        if (typeHeader.CharSet == null)
                        {
                            typeHeader.CharSet = "UTF-8";
                        }

                        content.Headers.ContentType = typeHeader;
                    }
                    #endregion

                    Task<HttpResponseMessage> _taskResponse = model.IsPost ? client.PostAsync(model.Url, content) : client.GetAsync(model.Url);

                    #region 等待异步执行完毕
                    while (_taskResponse.IsCompleted == false)
                    {
                        System.Threading.Thread.Sleep(50);
                    }

                    //检查请求完成后的状态
                    if (_taskResponse.IsCompleted)
                    {
                        if (_taskResponse.IsCanceled)
                        {
                            _paResponse.ErrCode = 4;
                            _paResponse.ErrMsg = "请求超时";
                            _paResponse.LogInfo = _paResponse.ErrMsg;
                        }
                        else if (_taskResponse.IsFaulted)
                        {
                            _paResponse.ErrCode = 3;
                            _paResponse.ErrMsg = "无法连接到目标地址";
                            _paResponse.LogInfo = _paResponse.ErrMsg;
                        }
                    }
                    #endregion

                    HttpResponseMessage response = null;

                    //成功执行请求
                    if (_taskResponse.Status == TaskStatus.RanToCompletion)
                    {
                        response = _taskResponse.Result;
                        _paResponse.HttpResponseMessage = response;

                        _paResponse.Result = _paResponse.HttpResponseMessage.Content.ReadAsStringAsync().Result;
                    }

                    //请求失败状态500，且获取头信息
                    if (response != null && !response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.RequestTimeout)
                        {
                            _paResponse.ErrCode = 2;
                            _paResponse.ErrMsg = "核心系统cookie失效（AskData  Found）";
                        }
                        else if (response.StatusCode == HttpStatusCode.NotModified)
                        {
                            _paResponse.ErrCode = 0;
                            _paResponse.ErrMsg = "NotModified";
                        }
                        else
                        {
                            _paResponse.ErrCode = 1;
                            _paResponse.ErrMsg = "请求数据失败";

                            string errorMsg = string.Empty;
                            var headers = response.Headers;

                            foreach (var header in headers)
                            {
                                if (header.Key == "x-iCore_fa-errorMsg")
                                {
                                    errorMsg += System.Web.HttpUtility.UrlDecode(((string[])(header.Value))[0]);
                                }
                            }
                        }
                    }
                    else if (response != null && response.IsSuccessStatusCode)
                    {
                        _paResponse.ErrCode = 0;
                        _paResponse.ErrMsg = "成功";

                    }
                }


            }
            catch (Exception ex)
            {
                _paResponse.ErrCode = 6;
                _paResponse.ErrMsg = "向平安请求失败:" + ex.Message;

            }
            return _paResponse;
        }
    }

}
