using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace CsLua.API
{
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
        LCSFunction = Function | 1 << 4,
        CSClosure = Function | 2 << 4,
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public readonly struct LuaInt
    {
        public static LuaInt MaxValue = Int64.MaxValue;
        public static LuaInt MinValue = Int64.MinValue;

        public static bool TryParse(string s, out LuaInt ret)
        {
            var flag = Int64.TryParse(s, out var i);
            ret = i;
            return flag;
        }

        public static bool TryParse(string s, NumberStyles numberStyles, NumberFormatInfo formatInfo, out LuaInt ret)
        {
            var flag = Int64.TryParse(s, numberStyles, formatInfo, out var i);
            ret = i;
            return flag;
        }

        public static implicit operator Int64(LuaInt i) => i.Value;
        public static implicit operator LuaInt(Int64 v) => new LuaInt(v);
        public static implicit operator LuaInt(LuaFloat v) => new LuaInt(v);

        [FieldOffset(0)] public readonly Int64 Value;

        public LuaInt(Int64 v)
        {
            Value = v;
        }

        public LuaInt(LuaFloat v)
        {
            Value = (Int64) v.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public string ToString(IFormatProvider? provider)
        {
            return Value.ToString(provider);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public readonly struct LuaUInt
    {
        public static LuaUInt MaxValue = UInt64.MaxValue;
        public static LuaUInt MinValue = UInt64.MinValue;

        public static implicit operator UInt64(LuaUInt i) => i.Value;
        public static implicit operator LuaUInt(UInt64 v) => new LuaUInt(v);
        public static implicit operator LuaUInt(Int64 v) => new LuaUInt(v);
        public static implicit operator LuaUInt(LuaInt v) => new LuaUInt(v.Value);

        [FieldOffset(0)] public readonly UInt64 Value;

        public LuaUInt(UInt64 v)
        {
            Value = v;
        }

        public LuaUInt(Int64 v)
        {
            Value = (ulong) v;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public string ToString(IFormatProvider? provider)
        {
            return Value.ToString(provider);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public readonly struct LuaFloat
    {
        public static LuaFloat MaxValue = double.MaxValue;
        public static LuaFloat MinValue = double.MinValue;

        public static bool IsPositiveInfinity(LuaFloat l)
        {
            return double.IsPositiveInfinity(l.Value);
        }

        public static bool IsNegativeInfinity(LuaFloat l)
        {
            return double.IsNegativeInfinity(l.Value);
        }

        public static LuaFloat Parse(string s)
        {
            return double.Parse(s);
        }

        public static bool TryParse(string s, out LuaFloat ret)
        {
            var flag = double.TryParse(s, out var d);
            ret = d;
            return flag;
        }

        public static bool IsNaN(LuaFloat l)
        {
            return double.IsNaN(l);
        }

        public static implicit operator double(LuaFloat f) => f.Value;
        public static implicit operator LuaFloat(double v) => new LuaFloat(v);
        public static implicit operator LuaFloat(int v) => new LuaFloat(v);
        public static implicit operator LuaFloat(LuaInt v) => new LuaFloat(v.Value);

        [FieldOffset(0)] public readonly double Value;

        public LuaFloat(double v)
        {
            Value = v;
        }

        public override string ToString()
        {
            return Value.ToString(CultureInfo.CurrentCulture);
        }

        public string ToString(IFormatProvider? provider)
        {
            return Value.ToString(provider);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public readonly struct LuaKContext
    {
        [FieldOffset(0)] public readonly Int64 Value;

        public LuaKContext(Int64 v)
        {
            Value = v;
        }

        public static implicit operator Int64(LuaKContext i) => i.Value;
        public static implicit operator LuaKContext(Int64 v) => new LuaKContext(v);
    }

    public static class LuaTypeEx
    {
        /// <summary>
        /// 获取无变体类型
        /// </summary>
        public static ELuaType GetNoVariantsType(this ELuaType type)
        {
            return (ELuaType) ((int) type & 0x0F);
        }

        public static bool IsNumber(this ELuaType type)
        {
            return type.GetNoVariantsType() == ELuaType.Number;
        }

        public static bool IsFunction(this ELuaType type)
        {
            return type.GetNoVariantsType() == ELuaType.Function;
        }

        public static bool IsUserdata(this ELuaType type)
        {
            return type == ELuaType.UserData || type == ELuaType.LightUserData;
        }

        public static bool IsCSFunction(this ELuaType type)
        {
            return type == ELuaType.LCSFunction;
        }
    }

    public enum EArithOp : byte
    {
        Add = 0,
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

    public delegate void LuaArith(ILuaState l, int op);

    public enum ECompOp : byte
    {
        Eq = 0,
        Lt,
        Le,
    }

    public delegate int LuaRawEqual(ILuaState l, int idx1, int idx2);

    public delegate int LuaCompare(ILuaState l, int idx1, int idx2, int op);

    public enum EStatus
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

        public static readonly string LUA_VERSION =
            $"Lua {LUA_VERSION_MAJOR}.{LUA_VERSION_MINOR}";

        public static readonly string LUA_RELEASE =
            $"{LUA_VERSION}.{LUA_RELEASE}";

        public static readonly string LUA_COPYRIGHT =
            "  Copyright (C) 1994-2018 Yang Ruihan";

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

        internal static readonly int MAXCCALLS = 200;
        internal static readonly int MAXUPVAL = 255;
    }

    /// <summary>
    /// 可注册到 Lua 的 C# Delegate
    /// </summary>
    public delegate int LuaCSFunction(ILuaState luaState);

    /// <summary>
    /// 持续运行函数 Delegate
    /// </summary>
    public delegate int LuaKFunction(ILuaState luaState, int status,
        LuaKContext ctx);

    public delegate string LuaReader(ILuaState l, object ud, int sz);

    public delegate int LuaWriter(ILuaState luaState, object p, int size,
        object ud);

    public struct LuaReg
    {
        public string Name;
        public LuaCSFunction Func;
    }

    public class LuaException : Exception
    {
        public EStatus Status { get; private set; }

        public LuaException(string msg, EStatus status) : base(msg)
        {
            Status = status;
        }
    }
}