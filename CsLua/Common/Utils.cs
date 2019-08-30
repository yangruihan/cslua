using System;

namespace CsLua.Common
{
    internal static class Utils
    {
        public static byte[] GetBytes(this string self)
        {
            var chars = self.ToCharArray();
            var len = chars.Length;
            var bytes = new byte[len];
            Array.Copy(chars, bytes, len);
            return bytes;
        }

        public static string ToStr(this byte[] self)
        {
            var len = self.Length;
            var chars = new char[len];
            Array.Copy(self, chars, len);
            return new string(chars);
        }
    }
}