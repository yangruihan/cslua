using CsLua.API;
using CsLua.Misc;

namespace CsLua.State
{
    internal partial class LuaState : ILuaState
    {
        public void SetTable(int idx)
        {
            var t = Stack[idx];
            var v = Stack.Pop();
            var k = Stack.Pop();
            InnerSetTable(t, k, v, false);
        }

        public void SetField(int idx, string k)
        {
            var t = Stack[idx];
            var v = Stack.Pop();
            InnerSetTable(t, new LuaValue(k, ELuaType.String), v, false);
        }

        public void RawSet(int idx)
        {
            var t = Stack[idx];
            var v = Stack.Pop();
            var k = Stack.Pop();
            InnerSetTable(t, k, v, true);
        }

        public void RawSetI(int idx, LuaInt i)
        {
            var t = Stack[idx];
            var v = Stack.Pop();
            InnerSetTable(t, new LuaValue(i), v, true);
        }

        public void RawSetP(int idx, object p)
        {
            var t = Stack[idx];
            var v = Stack.Pop();
            InnerSetTable(t, LuaValue.Create(p), v, true);
        }

        public void SetI(int idx, LuaInt i)
        {
            var t = Stack[idx];
            var v = Stack.Pop();
            InnerSetTable(t, new LuaValue(i), v, false);
        }

        public void SetGlobal(string name)
        {
            var t = Registry.Get(LuaConst.LUA_RIDX_GLOBALS);
            var v = Stack.Pop();
            InnerSetTable(t, new LuaValue(name, ELuaType.String), v, false);
        }

        public void Register(string name, LuaCSFunction f)
        {
            PushCSFunction(f);
            SetGlobal(name);
        }

        public void SetMetaTable(int idx)
        {
            var val = Stack[idx];
            var mtVal = Stack.Pop();
            if (mtVal is null)
            {
                LuaValue.SetMetaTable(val, null, this);
            }
            else if (mtVal.IsTable())
            {
                LuaValue.SetMetaTable(val, mtVal.GetTableValue(), this);
            }
            else
            {
                Debug.Panic("table expected!");
            }
        }

        public void SerUserValue(int idx)
        {
            var o = Stack[idx];
            LuaAPI.Check(this, o.IsUserData(), "full userdata expected");
            var v = Stack.Pop();
            (o.GetObjValue() as UserData)!.User = v;
        }

        private void InnerSetTable(LuaValue t, LuaValue k, LuaValue v, bool raw)
        {
            if (t.IsTable())
            {
                var table = t.GetTableValue();
                if (raw || !table.Get(k).IsNil() || !table.HasMetaField("__newindex"))
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
                    if (mf.IsTable())
                    {
                        InnerSetTable(mf, k, v, false);
                    }
                    else if (mf.IsFunction())
                    {
                        Stack.Push(mf);
                        Stack.Push(t);
                        Stack.Push(k);
                        Stack.Push(v);
                        Call(3, 0);
                        return;
                    }
                }
            }

            Debug.Panic("not a table!");
        }
    }
}