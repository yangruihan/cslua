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
            return InnerGetTable(t, k);
        }

        public ELuaType GetField(int idx, string key)
        {
            var t = _stack[idx];
            return InnerGetTable(t, new LuaValue(key));
        }

        public ELuaType GetI(int idx, LuaInt i)
        {
            var t = _stack[idx];
            return InnerGetTable(t, new LuaValue(i));
        }
        
        public ELuaType GetGlobal(string name)
        {
            var t = _registry.Get(Consts.LUA_RIDX_GLOBALS);
            return InnerGetTable(t, new LuaValue(name));
        }

        private ELuaType InnerGetTable(LuaValue t, LuaValue k)
        {
            if (t.Value is LuaTable table)
            {
                var v = table.Get(k);
                _stack.Push(v);
                return v.Type();
            }

            Debug.Panic("not a table!");
            return ELuaType.None;
        }
    }
}