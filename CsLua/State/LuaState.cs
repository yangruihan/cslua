using CsLua.API;

namespace CsLua.State
{
    partial class LuaState : ILuaState
    {
        private LuaStack _stack;

        public LuaState()
        {
            _stack = new LuaStack(20);
        }
    }
}