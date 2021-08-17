using System.Diagnostics;
using CsLua.State;

namespace CsLua.API
{
    public static class LuaAPI
    {
        [Conditional("LUA_ENABLE_ASSERT")]
        public static void Check(this ILuaState l, bool e, string msg)
        {
            (l as LuaState)!.Check(e, msg);
        }

        [Conditional("LUA_ENABLE_ASSERT")]
        public static void CheckNElems(this ILuaState l, int n)
        {
            (l as LuaState)!.CheckNElems(n);
        }

        public static void AdjustResults(this ILuaState l, int nRet)
        {
            var state = (l as LuaState)!;
            if (nRet == LuaConst.LUA_MULTRET && state.CallInfo.Top < state.Top)
                state.CallInfo.Top = state.Top;
        }

        internal static bool IsValid(LuaValue? v)
        {
            return v != null;
        }

        internal static bool IsPseudo(int i)
        {
            return i < LuaConst.LUA_REGISTRYINDEX;
        }

        internal static bool IsStackIndex(int i, LuaValue v)
        {
            return IsValid(v) && !IsPseudo(i);
        }
    }
}