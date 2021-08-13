using CsLua.API;

namespace CsLua.State
{
    internal partial class LuaState : ILuaState
    {
        public LuaCSFunction AtPanic(LuaCSFunction panicF)
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