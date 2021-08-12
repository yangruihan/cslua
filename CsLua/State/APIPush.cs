using CsLua.API;

namespace CsLua.State
{
    /// <summary>
    /// 将值推入栈中方法实现
    /// </summary>
    internal partial class LuaState : ILuaState
    {
        public void PushNil()
        {
            Stack.Push(LuaValue.Nil);
        }

        public void PushBoolean(bool b)
        {
            Stack.Push(b ? LuaValue.True : LuaValue.False);
        }

        public void PushInteger(long n)
        {
            Stack.Push(new LuaValue(n));
        }

        public void PushNumber(double n)
        {
            Stack.Push(new LuaValue(n));
        }

        public string PushString(string s)
        {
            Stack.Push(new LuaValue(s, ELuaType.String));
            return s;
        }

        public string PushFString(string fmt, params object[] args)
        {
            var s = string.Format(fmt, args);
            PushString(s);
            return s;
        }

        public void PushCSFunction(LuaCSFunction f)
        {
            Stack.Push(new Closure(f, 0));
        }

        public void PushGlobalTable()
        {
            var global = Registry.Get(LuaConst.LUA_RIDX_GLOBALS);
            Stack.Push(global);
        }

        public void PushCSClosure(LuaCSFunction f, int n)
        {
            var closure = new Closure(f, n);
            for (var i = n; i > 0; i--)
            {
                var val = Stack.Pop();
                closure.Upvals[i - 1] = new Upvalue {Val = val};
            }

            Stack.Push(closure);
        }

        public void PushLightUserdata(object userdata)
        {
            Stack.Push(new LuaValue(userdata, ELuaType.LightUserData));
        }

        public void PushThread()
        {
            Stack.Push(new LuaValue(this, ELuaType.Thread));
        }
    }
}