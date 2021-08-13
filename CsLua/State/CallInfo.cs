﻿using System;
using CsLua.API;

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

    internal unsafe struct YieldableLuaFunction
    {
        public int Base;
        public UInt32* SavedPc;
    }

    internal struct YieldableCSFunction
    {
        public LuaKFunction K;
        public Int64 OldErrFunc;
        public LuaKContext Ctx;
    }

    /// <summary>
    /// 调用信息
    /// </summary>
    internal class CallInfo
    {
        // 调用信息在堆栈中的偏移
        public int Func; // function index in the stack
        public int Top; // top for this function

        // 动态调用链表
        public CallInfo Previous;
        public CallInfo Next;

        public YieldableLuaFunction LuaFunction;
        public YieldableCSFunction CsFunction;

        public short NResults; // expected number of results from this function

        public CallInfoStatus CallStatus;

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