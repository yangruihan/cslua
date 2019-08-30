using CsLua.API;

namespace CsLua.State
{
    partial class LuaState : ILuaState
    {
        public void PushNil()
        {
            _stack.Push(LuaValue.Nil);
        }

        public void PushBoolean(bool b)
        {
            _stack.Push(b ? LuaValue.True : LuaValue.False);
        }

        public void PushInteger(long n)
        {
            _stack.Push(new LuaValue(n));
        }

        public void PushNumber(double n)
        {
            _stack.Push(new LuaValue(n));
        }

        public void PushString(string s)
        {
            _stack.Push(new LuaValue(s));
        }
    }
}