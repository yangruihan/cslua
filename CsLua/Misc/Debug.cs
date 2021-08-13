using System;

namespace CsLua.Misc
{
    public static class Debug
    {
        public static void Panic(string info)
        {
            throw new Exception(info);
        }
    }
}