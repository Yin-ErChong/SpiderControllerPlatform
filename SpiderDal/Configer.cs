using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiderUtil
{
    public class Configer
    {
        
        public static IConfiguration configuration;
        public static string GetValue(string key)
        {
            return configuration[key];
        }
        public static int TCP_Port => int.Parse(GetValue("TCPPort"));
    }
}
