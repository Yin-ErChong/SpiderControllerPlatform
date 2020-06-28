using System;
using System.Collections.Generic;
using System.Text;

namespace SpiderUtil
{
    public static class ByteArrayExtension
    {
        public static bool Equals(this byte[] bytes, byte[] otherBytes)
        {
            if (bytes.Length!= otherBytes.Length)
            {
                return false;
            }
            for (int i = 0; i < otherBytes.Length; i++)
            {
                if (otherBytes[i]!= bytes[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
