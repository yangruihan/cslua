using CsLua.API;

namespace CsLua.State
{
    internal partial class LuaState
    {
        public static void PreInitThread(LuaState l, GlobalState g)
        {
            l.GlobalState = g;
            l.Stack = null;
            l.Registry = null;
            l._registryShell = null;
        }

        public static LuaState NewThread(LuaState l)
        {
            var l1 = new LuaState(l);
            l.Stack.Push(l1);
            return l1;
        }

        public LuaTable Registry { get; private set; }

        private LuaValue _registryShell;

        public LuaStack Stack { get; private set; }

        public GlobalState GlobalState;

        public ELuaType LuaType => ELuaType.Thread;

        public LuaState(LuaState parent = null)
        {
            if (parent == null)
            {
                PreInitThread(this, new GlobalState(this));
            }
            else
            {
                PreInitThread(this, parent.GlobalState);
            }

            Registry = new LuaTable(0, 0);
            _registryShell = new LuaValue(Registry, ELuaType.Table);
            Registry.Put(LuaConst.LUA_RIDX_GLOBALS, new LuaTable(0, 0));
            PushLuaStack(new LuaStack(LuaConst.LUA_MINSTACK, this));
        }

        public void PushLuaStack(LuaStack stack)
        {
            stack.Prev = Stack;
            Stack = stack;
        }

        public void PopLuaStack()
        {
            var stack = Stack;
            Stack = stack.Prev;
            stack.Prev = null;
        }

        public LuaValue GetRegistry()
        {
            return _registryShell;
        }

        public void SetRegistry(LuaTable luaTable)
        {
            Registry = luaTable;
            _registryShell = new LuaValue(Registry, ELuaType.Table);
        }

        public int LuaUpvalueIndex(int i)
        {
            return LuaConst.LUA_REGISTRYINDEX - i;
        }
    }
}