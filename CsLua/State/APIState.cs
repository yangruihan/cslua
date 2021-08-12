using CsLua.API;

namespace CsLua.State
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    internal partial class LuaState : ILuaState
    {
        public CSFunction AtPanic(CSFunction panicF)
        {
            var old = GlobalState.Panic;
            GlobalState.Panic = panicF;
            return old;
        }

        public LuaFloat Version()
        {
            return GlobalState.Version;
        }
    }
}