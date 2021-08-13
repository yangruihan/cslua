using CsLua.Misc;
using CsLua.State;

namespace CsLua.API
{
    public class LuaAPI
    {
        public static void Check(ILuaState l, bool e, string msg)
        {
            Limits.Check(l, e, msg);
        }

        public static void CheckNElems(ILuaState l, int n)
        {
            var state = l as LuaState;
            Check(l, state!.Stack.Top >= n, "not enough elements in the stack");
        }
    }
}