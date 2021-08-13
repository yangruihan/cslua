using CsLua.API;

namespace CsLua.State
{
    internal partial class LuaState
    {
        public static void PreInitThread(LuaState l, GlobalState g)
        {
            l.GlobalState = g;
        }

        public static LuaState NewThread(LuaState l)
        {
            var l1 = new LuaState(l);
            l.Stack.Push(l1);
            return l1;
        }

        public EErrorCode Status;

        public LuaStack Stack { get; private set; }

        public GlobalState GlobalState { get; private set; }

        public CallInfo CallInfo;

        public ELuaType LuaType => ELuaType.Thread;

        public LuaTable Registry => GlobalState.RegistryTable;

        public LuaState(LuaState? parent = null)
        {
            Status = EErrorCode.Ok;
            PreInitThread(this, parent == null ? new GlobalState(this) : parent.GlobalState);

            GlobalState.Init(this);

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
            return GlobalState.Registry;
        }

        public void SetRegistry(LuaTable luaTable)
        {
            GlobalState.SetRegistry(luaTable);
        }

        public int LuaUpvalueIndex(int i)
        {
            return LuaConst.LUA_REGISTRYINDEX - i;
        }
    }
}