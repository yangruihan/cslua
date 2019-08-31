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
            InnerSetTable(t, k, v);
        }

        public void SetField(int idx, string k)
        {
            var t = _stack[idx];
            var v = _stack.Pop();
            InnerSetTable(t, new LuaValue(k), v);
        }

        public void SetI(int idx, LuaInt i)
        {
            var t = _stack[idx];
            var v = _stack.Pop();
            InnerSetTable(t, new LuaValue(i), v);
        }

        private void InnerSetTable(LuaValue t, LuaValue k, LuaValue v)
        {
            if (t.Value is LuaTable table)
            {
                table.Put(k, v);
                return;
            }

            Debug.Panic("not a table!");
        }
    }
}