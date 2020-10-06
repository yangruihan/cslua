using System;
using System.Text;
using CsLua.VM;

namespace CsLua.Common
{
    internal static class Utils
    {
        public static byte[] GetBytes(this string self)
        {
            return Encoding.UTF8.GetBytes(self);
        }

        public static string ToStr(this byte[] self)
        {
            return Encoding.UTF8.GetString(self);
        }

        public static T[] Slice<T>(this T[] self, int start = 0, int end = -1)
        {
            if (end == -1 || end > self.Length)
                end = self.Length;

            if (end < start)
                return null;

            var len = end - start;
            var ret = new T[len];
            Array.Copy(self, start, ret, 0, len);
            return ret;
        }

        public static int ToInt(this EOpCode code)
        {
            return (int) code;
        }
    }
}