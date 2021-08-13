using CsLua.API;

namespace CsLua.Number
{
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