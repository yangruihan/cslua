using System;
using CsLua.API;
using CsLua.Misc;

namespace CsLua.State
{
    internal partial class LuaState : ILuaState
    {
        public void NewTable()
        {
            CreateTable(0, 0);
        }

        public void CreateTable(int nArr, int nRec)
        {
            var t = new LuaTable(nArr, nRec);
            Stack.Push(t);
        }

        public IntPtr NewUserData(int size)
        {
            var v = LuaValue.CreateUserData(size);
            Stack.Push(v);
            return v.GetObjValue() is UserData u
                ? u.Memory
                : default;
        }

        public ELuaType GetTable(int idx)
        {
            var t = Stack[idx];
            var k = Stack.Pop();
            return InnerGetTable(t, k, false).GetParentType();
        }

        public ELuaType GetField(int idx, string key)
        {
            var t = Stack[idx];
            return InnerGetTable(t, new LuaValue(key, ELuaType.String), false)
                .GetParentType();
        }

        public ELuaType RawGet(int idx)
        {
            var t = Stack[idx];
            var k = Stack.Pop();
            return InnerGetTable(t, k, true).GetParentType().GetParentType();
        }

        public ELuaType RawGetI(int idx, LuaInt i)
        {
            var t = Stack[idx];
            return InnerGetTable(t, new LuaValue(i), true).GetParentType();
        }

        public ELuaType RawGetP(int idx, object p)
        {
            var t = Stack[idx];
            LuaAPI.Check(this, t.IsTable(), "table expected");
            return InnerGetTable(t, LuaValue.Create(p), true).GetParentType();
        }

        public ELuaType GetI(int idx, LuaInt i)
        {
            var t = Stack[idx];
            return InnerGetTable(t, new LuaValue(i), false).GetParentType();
        }

        public ELuaType GetGlobal(string name)
        {
            var t = Registry.Get(LuaConst.LUA_RIDX_GLOBALS);
            return InnerGetTable(t, new LuaValue(name, ELuaType.String), false)
                .GetParentType();
        }

        public bool GetMetaTable(int idx)
        {
            var val = Stack[idx];
            var mt = LuaValue.GetMetaTable(val, this);
            if (mt != null)
            {
                Stack.Push(mt);
                return true;
            }

            return false;
        }

        public ELuaType GetUserValue(int idx)
        {
            var val = Stack[idx];
            LuaAPI.Check(this, val.IsUserData(), "full userdata expected");
            Stack.Push(val);
            return val.Type.GetParentType();
        }

        private ELuaType InnerGetTable(LuaValue t, LuaValue k, bool raw)
        {
            if (t.IsTable())
            {
                var table = t.GetTableValue();
                var v = table.Get(k);

                if (raw || !v.IsNil() || !table.HasMetaField("__index"))
                {
                    Stack.Push(v);
                    return v.Type;
                }
            }

            if (!raw)
            {
                var mf = LuaValue.GetMetaField(t, "__index", this);
                if (mf != null)
                {
                    if (mf.IsTable())
                    {
                        return InnerGetTable(mf, k, false);
                    }

                    if (mf.IsFunction())
                    {
                        Stack.Push(mf);
                        Stack.Push(t);
                        Stack.Push(k);
                        Call(2, 1);
                        var v = Stack.Get(-1);
                        return v.Type;
                    }
                }
            }

            Debug.Panic("index error!");
            return ELuaType.None;
        }
    }
}