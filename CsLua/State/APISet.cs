using CsLua.API;
using CsLua.Common;

namespace CsLua.State
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    partial class LuaState : ILuaState
    {
        public void SetTable(int idx)
        {
            var t = _stack[idx];
            var v = _stack.Pop();
            var k = _stack.Pop();
            InnerSetTable(t, k, v, false);
        }

        public void SetField(int idx, string k)
        {
            var t = _stack[idx];
            var v = _stack.Pop();
            InnerSetTable(t, new LuaValue(k), v, false);
        }

        public void RawSet(int idx)
        {
            var t = _stack[idx];
            var v = _stack.Pop();
            var k = _stack.Pop();
            InnerSetTable(t, k, v, true);
        }

        public void RawSetI(int idx, long i)
        {
            var t = _stack[idx];
            var v = _stack.Pop();
            InnerSetTable(t, new LuaValue(i), v, true);
        }

        public void SetI(int idx, LuaInt i)
        {
            var t = _stack[idx];
            var v = _stack.Pop();
            InnerSetTable(t, new LuaValue(i), v, false);
        }

        public void SetGlobal(string name)
        {
            var t = _registry.Get(Consts.LUA_RIDX_GLOBALS);
            var v = _stack.Pop();
            InnerSetTable(t, new LuaValue(name), v, false);
        }

        public void Register(string name, CSFunction f)
        {
            PushCSFunction(f);
            SetGlobal(name);
        }

        public void SetMetaTable(int idx)
        {
            var val = _stack[idx];
            var mtVal = _stack.Pop();
            if (mtVal is null)
            {
                LuaValue.SetMetaTable(val, null, this);
            }
            else if (mtVal.Value is LuaTable lt)
            {
                LuaValue.SetMetaTable(val, lt, this);
            }
            else
            {
                Debug.Panic("table expected!");
            }
        }

        private void InnerSetTable(LuaValue t, LuaValue k, LuaValue v, bool raw)
        {
            if (t.Value is LuaTable table)
            {
                if (raw || table.Get(k).Value != null || !table.HasMetaField("__newindex"))
                {
                    table.Put(k, v);
                    return;
                }
            }

            if (!raw)
            {
                var mf = LuaValue.GetMetaField(t, "__newindex", this);
                if (mf != null)
                {
                    if (mf.Value is LuaTable lt)
                    {
                        InnerSetTable(mf, k, v, false);
                    }
                    else if (mf.Value is Closure c)
                    {
                        _stack.Push(mf);
                        _stack.Push(t);
                        _stack.Push(k);
                        _stack.Push(v);
                        Call(3, 0);
                        return;
                    }
                }
            }

            Debug.Panic("not a table!");
        }
    }
}