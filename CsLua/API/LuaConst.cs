using System;

namespace CsLua.API
{
    using LuaInt = Int64;
    using LuaFloat = Double;
    using LuaContext = Int64;

    public enum ELuaType
    {
        None = -1,
        Nil = 0,
        Boolean,
        LightUserData,
        Number,
        String,
        Table,
        Function,
        UserData,
        Thread,

        NumTags,

        Float = Number | 0 << 4,
        Int = Number | 1 << 4,

        Closure = Function | 0 << 4,
        CSFunction = Function | 1 << 4,
    }

    public static class LuaTypeEx
    {
        public static ELuaType GetParentType(this ELuaType type)
        {
            return (ELuaType) ((int) type & 0b1111);
        }

        public static bool IsNumber(this ELuaType type)
        {
            return type.GetParentType() == ELuaType.Number;
        }

        public static bool IsFunction(this ELuaType type)
        {
            return type.GetParentType() == ELuaType.Function;
        }

        public static bool IsUserdata(this ELuaType type)
        {
            return type == ELuaType.UserData || type == ELuaType.LightUserData;
        }
    }

    public enum EArithOp : byte
    {
        Add,
        Sub,
        Mul,
        Mod,
        Pow,
        Div,
        IDiv,
        BAnd,
        BOr,
        BXor,
        Shl,
        Shr,
        Unm,
        BNot,
    }

    public enum ECompOp : byte
    {
        Eq,
        Lt,
        Le,
    }

    public enum EErrorCode
    {
        Undefine = -1,
        Ok = 0,
        Yield = 1,
        ErrRun = 2,
        ErrSyntax = 3,
        ErrMem = 4,
        ErrGcMm = 5,
        ErrErr = 6,
    }

    public static class LuaConst
    {
        // ----- 版本相关 -----
        public static readonly string LUA_VERSION_MAJOR = "5";
        public static readonly string LUA_VERSION_MINOR = "3";
        public static readonly int LUA_VERSION_NUM = 503;
        public static readonly string LUA_VERSION_RELEASE = "5";

        public static readonly string LUA_VERSION = $"Lua {LUA_VERSION_MAJOR}.{LUA_VERSION_MINOR}";
        public static readonly string LUA_RELEASE = $"{LUA_VERSION}.{LUA_RELEASE}";
        public static readonly string LUA_COPYRIGHT = "  Copyright (C) 1994-2018 Yang Ruihan";
        public static readonly string LUA_AUTHORS = "Yang Ruihan";

        // Lua 签名，4个字节,用于校验读取的 Chunk 是否合法
        public static readonly string LUA_SIGNATURE = "\x1bLua";

        // lua_pcall 和 lua_call 中返回多值选项
        public static readonly int LUA_MULTRET = -1;

        // 伪指数设计
        // -LUAI_MAXSTACK 是最小合法索引，这里再 -1000 是为了保证充足的空间进行栈溢出检测
        public static readonly int LUA_REGISTRYINDEX = -LUAI_MAXSTACK - 1000;

        // 为 CSFunction 提供的最小可用栈
        public static readonly int LUA_MINSTACK = 20;
        public static readonly int LUAI_MAXSTACK = 1000000;

        public static readonly Int64 LUA_RIDX_MAINTHREAD = 1;
        public static readonly Int64 LUA_RIDX_GLOBALS = 2;
        public static readonly Int64 LUA_RIDX_LAST = LUA_RIDX_GLOBALS;

        public static readonly Int64 LUA_MAXINTEGER = Int64.MaxValue;
    }

    /// <summary>
    /// 可注册到 Lua 的 C# Delegate
    /// </summary>
    public delegate int CSFunction(ILuaState luaState);

    /// <summary>
    /// 持续运行函数 Delegate
    /// </summary>
    public delegate int KFunction(ILuaState luaState, int status, LuaContext ctx);

    public delegate string LuaReader(ILuaState l, object ud, int sz);

    public delegate int LuaWriter(ILuaState luaState, object p, int size, object ud);

    public struct LuaReg
    {
        public string Name;
        public CSFunction Func;
    }
}