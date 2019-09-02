using CsLua.API;
using CsLua.Binchunk;

namespace CsLua.State
{
    partial class LuaState
    {
        private LuaStack _stack;

        public LuaState()
        {
            _stack = new LuaStack(20);
        }

        public void PushLuaStack(LuaStack stack)
        {
            stack.Prev = _stack;
            _stack = stack;
        }

        public void PopLuaStack()
        {
            var stack = _stack;
            _stack = stack.Prev;
            stack.Prev = null;
        }
    }
}