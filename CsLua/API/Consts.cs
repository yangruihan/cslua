using System;

namespace CsLua.API
{
    public enum ELuaType
    {
        None = -1,
        Nil,
        Boolean,
        LightUserData,
        Number,
        String,
        Table,
        Function,
        UserData,
        Thread,
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

    public static class Consts
    {
        public const int LUA_MINSTACK = 20;
        public const int LUAI_MAXSTACK = 1000000;
        public const int LUA_REGISTRYINDEX = -LUAI_MAXSTACK - 1000;
        public const Int64 LUA_RIDX_GLOBALS = 2;
    }

    public enum EErrorCode
    {
        Ok,
        Yield,
        ErrRun,
        ErrSyntax,
        ErrMem,
        ErrGcMm,
        ErrErr,
        ErrFile,
    }
}