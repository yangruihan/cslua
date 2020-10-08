using CsLua.API;

namespace CsLua.State
{
    partial class LuaState
    {
        private LuaTable _registry;
        public LuaTable Registry => _registry;

        private LuaValue _registryShell;

        private LuaStack _stack;
        public LuaStack Stack => _stack;

        public LuaState()
        {
            _registry = new LuaTable(0, 0);
            _registryShell = new LuaValue(_registry, ELuaType.Table);
            _registry.Put(Consts.LUA_RIDX_GLOBALS, new LuaTable(0, 0));
            PushLuaStack(new LuaStack(Consts.LUA_MINSTACK, this));
        }

        public void PushLuaStack(LuaStack stack)
        {
            stack.Prev = _stack;
            _stack = stack;
        }

        public void PopLuaStack()
        {
            var stack = _stack;
            _stack = stack.Prev;
            stack.Prev = null;
        }

        public LuaValue GetRegistry()
        {
            return _registryShell;
        }
        
        public void SetRegistry(LuaTable luaTable)
        {
            _registry = luaTable;
            _registryShell = new LuaValue(_registry, ELuaType.Table);
        }
        
        public int LuaUpvalueIndex(int i)
        {
            return Consts.LUA_REGISTRYINDEX - i;
        }
    }
}