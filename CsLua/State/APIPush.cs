using CsLua.API;

namespace CsLua.State
{
    /// <summary>
    /// 将值推入栈中方法实现
    /// </summary>
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
            _stack.Push(new LuaValue(s, ELuaType.String));
        }

        public void PushCSFunction(CSFunction f)
        {
            _stack.Push(new Closure(f, 0));
        }

        public void PushGlobalTable()
        {
            var global = _registry.Get(Consts.LUA_RIDX_GLOBALS);
            _stack.Push(global);
        }

        public void PushCSClosure(CSFunction f, int n)
        {
            var closure = new Closure(f, n);
            for (var i = n; i > 0; i--)
            {
                var val = _stack.Pop();
                closure.Upvals[i - 1] = new Upvalue {Val = val};
            }

            _stack.Push(closure);
        }
    }
}