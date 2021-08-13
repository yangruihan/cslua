using CsLua.API;

namespace CsLua.Misc
{
    internal static class Debug
    {
        public static void Panic(string info)
        {
            throw new LuaException(info);
        }
    }
}