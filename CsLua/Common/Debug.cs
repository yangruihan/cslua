using System;

namespace CsLua.Common
{
    public static class Debug
    {
        public static void Error(string info)
        {
            throw new Exception(info);
        }
    }
}