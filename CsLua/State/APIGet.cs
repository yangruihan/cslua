using CsLua.API;
using CsLua.Common;

namespace CsLua.State
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

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

        public ELuaType GetTable(int idx)
        {
            var t = Stack[idx];
            var k = Stack.Pop();
            return InnerGetTable(t, k, false);
        }

        public ELuaType GetField(int idx, string key)
        {
            var t = Stack[idx];
            return InnerGetTable(t, new LuaValue(key, ELuaType.String), false);
        }

        public ELuaType RawGet(int idx)
        {
            var t = Stack[idx];
            var k = Stack.Pop();
            return InnerGetTable(t, k, true);
        }

        public ELuaType RawGetI(int idx, long i)
        {
            var t = Stack[idx];
            return InnerGetTable(t, new LuaValue(i), true);
        }

        public ELuaType GetI(int idx, LuaInt i)
        {
            var t = Stack[idx];
            return InnerGetTable(t, new LuaValue(i), false);
        }

        public ELuaType GetGlobal(string name)
        {
            var t = Registry.Get(LuaConst.LUA_RIDX_GLOBALS);
            return InnerGetTable(t, new LuaValue(name, ELuaType.String), false);
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