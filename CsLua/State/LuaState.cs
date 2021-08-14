using System;
using CsLua.API;

namespace CsLua.State
{
    internal partial class LuaState
    {
        public static int BASIC_STACK_SIZE = 2 * LuaConst.LUA_MINSTACK;

        public static void PreInitThread(LuaState l, GlobalState g)
        {
            l.GlobalState = g;
            l.NNy = 1;
            l.NCcalls = 0;
        }

        public static LuaState NewThread(LuaState l)
        {
            var l1 = new LuaState(l);
            l.Stack.Push(l1);
            return l1;
        }

        public EStatus Status;

        public LuaStack Stack { get; private set; }
        public int Top => Stack.Top;

        public GlobalState GlobalState { get; private set; }

        public readonly CallInfo BaseCi;
        public CallInfo CallInfo;

        public Int64 ErrFunc;

        public ushort NNy; // number of non-yieldable calls in stack
        public ushort NCcalls; // number of nested C calls 

        public ELuaType LuaType => ELuaType.Thread;

        public LuaTable Registry => GlobalState.RegistryTable;

        public LuaState(LuaState? parent = null)
        {
            Status = EStatus.Ok;
            PreInitThread(this, parent == null ? new GlobalState(this) : parent.GlobalState);

            GlobalState.Init(this);

            PushLuaStack(new LuaStack(BASIC_STACK_SIZE, this));

            BaseCi = new CallInfo();
            CallInfo = BaseCi;
            CallInfo.CallStatus = CallInfoStatus.INIT;
            CallInfo.Func = Top;
            PushNil(); // 'function' entry for this 'ci'
            CallInfo.Top = Top + LuaConst.LUA_MINSTACK;
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

        private LuaValue? Index2Addr(int idx)
        {
            return Index2Addr(idx);
        }

        private LuaValue? Index2Addr(int idx, out int absIdx)
        {
            if (idx > 0)
            {
                absIdx = CallInfo.Func + idx;
                var value = Stack[absIdx];
                Check(idx <= CallInfo.Top - (CallInfo.Func + 1), "unacceptable index");
                return value;
            }
            else if (!LuaAPI.IsPseudo(idx)) // negative idx
            {
                Check(idx != 0 && -idx <= Top - (CallInfo.Func + 1), "invalid index");
                absIdx = Top + idx;
                return Stack[absIdx];
            }
            else if (idx == LuaConst.LUA_REGISTRYINDEX) // registry
            {
                absIdx = LuaConst.LUA_REGISTRYINDEX;
                return GlobalState.Registry;
            }
            else // upvalues
            {
                absIdx = LuaConst.LUA_REGISTRYINDEX - idx;
                Check(absIdx <= LuaConst.MAXUPVAL + 1, "upvalue index too large");
                if (Stack[CallInfo.Func].IsLCSFunction()) // light CSFunction has no upvalues
                {
                    return null;
                }
                else
                {
                    var c = Stack[CallInfo.Func].GetClosureValue();
                    return absIdx <= c.Upvals.Length ? c.Upvals[absIdx - 1].Val : null;
                }
            }
        }
    }
}