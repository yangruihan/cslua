using System;
using System.Collections.Generic;
using CsLua.API;
using CsLua.Binchunk;

namespace CsLua.State
{
    [Flags]
    internal enum CallInfoStatus : short
    {
        INIT = 0,
        OAH = 1 << 0, // original value of 'allowhook'
        LUA = 1 << 1, // call is running a Lua function
        HOOKED = 1 << 2, // call is running a debug hook
        FRESH = 1 << 3, // call is running on a fresh invocation of luaV_execute
        YPCALL = 1 << 4, // call is a yieldable protected call
        TAIL = 1 << 5, // call was tail called
        HOOKYIELD = 1 << 6, // last hook called yielded
        LEQ = 1 << 7, // using __lt for __le
        FIN = 1 << 8, // call is running a finalizer
    }

    /// <summary>
    /// 调用信息
    /// </summary>
    internal class CallInfo
    {
        public struct _LuaClosure
        {
            public LuaClosure Closure;
            public LuaValue[] Varargs;
            public Dictionary<int, Upvalue> Openuvs;
            public int SavedPc;
        }

        public struct _CSFunction
        {
            public LuaKFunction K;
            public Int64 OldErrFunc;
            public LuaKContext Ctx;
        }

        // 调用信息在堆栈中的偏移
        public int Func; // function index in the stack
        public int Top; // top for this function

        // 动态调用链表
        public CallInfo? Previous;
        public CallInfo? Next;

        public _LuaClosure LuaClosure;
        public _CSFunction CsFunction;

        public int NResults; // expected number of results from this function

        public CallInfoStatus CallStatus;

        public CallInfo(CallInfo? parent)
        {
            if (parent != null)
            {
                parent.Next = this;
                this.Previous = parent;
                this.Next = null;
            }
        }

        public bool IsLua()
        {
            return (CallStatus & CallInfoStatus.LUA) == CallInfoStatus.LUA;
        }
    }

    internal static class CallInfoUtils
    {
        public static bool IsLua(this CallInfo? callInfo)
        {
            return callInfo != null && callInfo.IsLua();
        }
    }
}