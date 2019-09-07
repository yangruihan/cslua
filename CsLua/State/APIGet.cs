using CsLua.API;
using CsLua.Common;

namespace CsLua.State
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    partial class LuaState : ILuaState
    {
        public void NewTable()
        {
            CreateTable(0, 0);
        }

        public void CreateTable(int nArr, int nRec)
        {
            var t = new LuaTable(nArr, nRec);
            _stack.Push(t);
        }

        public ELuaType GetTable(int idx)
        {
            var t = _stack[idx];
            var k = _stack.Pop();
            return InnerGetTable(t, k, false);
        }

        public ELuaType GetField(int idx, string key)
        {
            var t = _stack[idx];
            return InnerGetTable(t, new LuaValue(key), false);
        }

        public ELuaType RawGet(int idx)
        {
            var t = _stack[idx];
            var k = _stack.Pop();
            return InnerGetTable(t, k, true);
        }

        public ELuaType RawGetI(int idx, long i)
        {
            var t = _stack[idx];
            return InnerGetTable(t, new LuaValue(i), true);
        }

        public ELuaType GetI(int idx, LuaInt i)
        {
            var t = _stack[idx];
            return InnerGetTable(t, new LuaValue(i), false);
        }

        public ELuaType GetGlobal(string name)
        {
            var t = _registry.Get(Consts.LUA_RIDX_GLOBALS);
            return InnerGetTable(t, new LuaValue(name), false);
        }

        public bool GetMetaTable(int idx)
        {
            var val = _stack[idx];
            var mt = LuaValue.GetMetaTable(val, this);
            if (mt != null)
            {
                _stack.Push(mt);
                return true;
            }

            return false;
        }

        private ELuaType InnerGetTable(LuaValue t, LuaValue k, bool raw)
        {
            if (t.Value is LuaTable table)
            {
                var v = table.Get(k);

                if (raw || v != null || !table.HasMetaField("__index"))
                {
                    _stack.Push(v);
                    return v.Type();
                }
            }

            if (!raw)
            {
                var mf = LuaValue.GetMetaField(t, "__index", this);
                if (mf != null)
                {
                    if (mf.Value is LuaTable lt)
                    {
                        return InnerGetTable(mf, k, false);
                    }
                    else if (mf.Value is Closure c)
                    {
                        _stack.Push(mf);
                        _stack.Push(t);
                        _stack.Push(k);
                        Call(2, 1);
                        var v = _stack.Get(-1);
                        return v.Type();
                    }
                }
            }

            Debug.Panic("index error!");
            return ELuaType.None;
        }
    }
}