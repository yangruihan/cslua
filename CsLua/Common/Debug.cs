using System;

namespace CsLua.Common
{
    public static class Debug
    {
        public static void Panic(string info)
        {
            throw new Exception(info);
        }
    }
}