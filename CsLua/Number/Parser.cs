using System;

namespace CsLua.Number
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    internal static class Parser
    {
        public static bool ParseInteger(string str, out LuaInt ret)
        {
            return LuaInt.TryParse(str, out ret);
        }

        public static bool ParseFloat(string str, out LuaFloat ret)
        {
            return LuaFloat.TryParse(str, out ret);
        }
    }
}