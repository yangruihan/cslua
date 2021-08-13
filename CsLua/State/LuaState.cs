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

        private LuaValue Index2Addr(int idx)
        {
            if (idx > 0)
            {
                var value = Stack[CallInfo.Func + idx];
                LuaAPI.Check(this, idx <= CallInfo.Top - (CallInfo.Func + 1), "unacceptable index");
                if (value == null)
                    return LuaValue.Nil;
                return value;
            }
            else if (!LuaAPI.IsPseudo(idx)) // negative idx
            {
                LuaAPI.Check(this, idx != 0 && -idx <= Top - (CallInfo.Func + 1), "invalid index");
                return Stack[Top + idx];
            }
            else if (idx == LuaConst.LUA_REGISTRYINDEX) // registry
            {
                return GlobalState.Registry;
            }
            else // upvalues
            {
                idx = LuaConst.LUA_REGISTRYINDEX - idx;
                LuaAPI.Check(this, idx <= LuaConst.MAXUPVAL + 1, "upvalue index too large");
                if (Stack[CallInfo.Func].IsCSFunction()) // CSFunction has no upvalues
                {
                    return LuaValue.Nil;
                }
                else
                {
                    var c = Stack[CallInfo.Func].GetClosureValue();
                    return idx <= c.Upvals.Length ? c.Upvals[idx - 1].Val : LuaValue.Nil;
                }
            }
        }
    }
}