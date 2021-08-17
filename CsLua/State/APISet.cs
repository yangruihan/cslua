using CsLua.API;
using CsLua.Misc;

namespace CsLua.State
{
    internal partial class LuaState : ILuaState
    {
        public void SetTable(int idx)
        {
            CheckNElems(2);
            var t = GetValueByRelIdx(idx)!;
            var v = Pop()!;
            var k = Pop()!;
            InnerSetTable(t, k, v, false);
        }

        public void SetField(int idx, string k)
        {
            var t = GetValueByRelIdx(idx)!;
            var v = Pop()!;
            InnerSetTable(t, new LuaValue(k), v, false);
        }

        public void RawSet(int idx)
        {
            CheckNElems(2);
            var t = GetValueByRelIdx(idx)!;
            Check(t.IsTable(), "table expected");
            var v = Pop()!;
            var k = Pop()!;
            InnerSetTable(t, k, v, true);
        }

        public void RawSetI(int idx, LuaInt i)
        {
            CheckNElems(1);
            var t = GetValueByRelIdx(idx)!;
            var v = Pop()!;
            InnerSetTable(t, new LuaValue(i), v, true);
        }

        public void RawSetP(int idx, object p)
        {
            CheckNElems(1);
            var t = GetValueByRelIdx(idx)!;
            Check(t.IsTable(), "table expected");
            var v = Pop()!;
            InnerSetTable(t, new LuaValue(p, ELuaType.LightUserData), v, true);
        }

        public void SetI(int idx, LuaInt i)
        {
            CheckNElems(1);
            var t = GetValueByRelIdx(idx)!;
            var v = Pop()!;
            InnerSetTable(t, new LuaValue(i), v, false);
        }

        public void SetGlobal(string name)
        {
            var t = Registry.Get(LuaConst.LUA_RIDX_GLOBALS);
            var v = Stack.Pop()!;
            InnerSetTable(t, new LuaValue(name), v, false);
        }

        public void Register(string name, LuaCSFunction f)
        {
            PushCSFunction(f);
            SetGlobal(name);
        }

        public bool SetMetaTable(int idx)
        {
            CheckNElems(1);
            var val = GetValueByRelIdx(idx)!;
            var mtVal = Pop()!;
            if (mtVal is null || mtVal.IsNil())
            {
                LuaValue.SetMetaTable(val, null, this);
            }
            else if (mtVal.IsTable())
            {
                LuaValue.SetMetaTable(val, mtVal.GetTableValue(), this);
            }
            else
            {
                Check(false, "table expected");
            }
            return true;
        }

        public void SerUserValue(int idx)
        {
            CheckNElems(1);
            var o = GetValueByRelIdx(idx)!;
            Check(o.IsFullUserData(), "full userdata expected");
            var v = Pop();
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