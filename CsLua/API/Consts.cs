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

    public static class Consts
    {
        public const string LUA_PROMPT = "> ";
        public const string LUA_PROMPT2 = ">> ";

        public const int LUA_MINSTACK = 20;
        public const int LUAI_MAXSTACK = 1000000;
        public const int LUA_REGISTRYINDEX = -LUAI_MAXSTACK - 1000;
        public const Int64 LUA_RIDX_GLOBALS = 2;

        public const Int64 LUA_MAXINTEGER = Int64.MaxValue;
    }

    public enum EErrorCode
    {
        Undefine = -1,
        Ok = 0,
        Yield,
        ErrRun,
        ErrSyntax,
        ErrMem,
        ErrGcMm,
        ErrErr,
        ErrFile,
    }

    public delegate int CSFunction(ILuaState luaState);

    public struct LuaReg
    {
        public string Name;
        public CSFunction Func;
    }
}