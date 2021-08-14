using System.Diagnostics;
using CsLua.API;
using Debug = CsLua.Misc.Debug;

namespace CsLua.State
{
    internal partial class LuaState
    {
        [Conditional("LUA_ENABLE_ASSERT")]
        internal void Check(bool e, string msg)
        {
            Debug.Assert(e, msg);
        }

        [Conditional("LUA_ENABLE_ASSERT")]
        internal void CheckNElems(int n)
        {
            Check(n < Top - CallInfo.Func, "not enough elements in the stack");
        }

        [Conditional("LUA_ENABLE_ASSERT")]
        internal void CheckStackIndex(int i, LuaValue v)
        {
            Check(LuaAPI.IsStackIndex(i, v), "index not in the stack");
        }

        [Conditional("LUA_ENABLE_ASSERT")]
        internal void CheckValidIndex(LuaValue v)
        {
            Check(LuaAPI.IsValid(v), "invalid index");
        }
    }
}