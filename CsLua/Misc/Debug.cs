using System.Diagnostics;
using CsLua.API;

namespace CsLua.Misc
{
    internal static class Debug
    {
        public static void Panic(string info, EStatus status = EStatus.Undefine)
        {
            throw new LuaException(info, status);
        }

        [Conditional("LUA_ENABLE_ASSERT")]
        internal static void Assert(bool cond, string msg = "")
        {
            System.Diagnostics.Debug.Assert(cond, msg);
        }
    }
}