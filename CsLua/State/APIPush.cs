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
            Push(LuaValue.Nil);
        }

        public void PushBoolean(bool b)
        {
            Push(b ? LuaValue.True : LuaValue.False);
        }

        public void PushInteger(LuaInt n)
        {
            Push(new LuaValue(n));
        }

        public void PushNumber(LuaFloat n)
        {
            Push(new LuaValue(n));
        }

        public string PushString(string s)
        {
            Push(new LuaValue(s, ELuaType.String));
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
            Push(new LuaValue(f, ELuaType.LCSFunction));
        }

        public void PushGlobalTable()
        {
            var global = Registry.Get(LuaConst.LUA_RIDX_GLOBALS);
            Push(global);
        }

        public void PushCSClosure(LuaCSFunction f, int n)
        {
            var closure = new CSClosure(f, n);
            for (var i = n; i > 0; i--)
            {
                var val = Stack.Pop();
                closure.Upvals![i - 1] = new Upvalue {Val = val};
            }

            Push(new LuaValue(closure, ELuaType.CSClosure));
        }

        public void PushLightUserdata(object userdata)
        {
            Push(new LuaValue(userdata, ELuaType.LightUserData));
        }

        public void PushThread()
        {
            Push(new LuaValue(this, ELuaType.Thread));
        }
    }
}