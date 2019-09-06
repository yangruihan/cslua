using CsLua.API;
using CsLua.Binchunk;

namespace CsLua.State
{
    partial class LuaState
    {
        private LuaTable _registry;
        private LuaValue _registryShell;

        private LuaStack _stack;

        public LuaState()
        {
            _registry = new LuaTable(0, 0);
            _registryShell = new LuaValue(_registry);
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
            _registryShell = new LuaValue(_registryShell);
        }
    }
}