using System;
using CsLua.Common;

namespace CsLua.Binchunk
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    /// <summary>
    /// Lua 二进制预编译块常量部分
    /// </summary>
    public class ChunkConst
    {
        public const string LUA_SIGNATURE = "\u001bLua"; // Lua 签名，4个字节,用于校验读取的 Chunk 是否合法
        public const byte LUAC_VERSION = 0x53; // 版本号，1个字节，由三部分组成，大版本号（Major Version）、小版本号（Minor Version）、发布号（Release Version），值为大版本号乘以 16 加小版本号
        public const byte LUAC_FORMAT = 0; // 格式号，1个字节，校验用，Lua 官方实现使用的格式号是 0
        public const string LUAC_DATA = "\u0019\u0093\u000d\u000a\u001a\u000a"; // 6个字节，校验用，前两个字节为 0x1993，后面字节依次是 0x0d（回车）、0x0a（换行）、0x1a（替换符）、0x0a（换行符）
        public const byte CINT_SIZE = 4; // c int 数据类型内存占用大小，一个字节
        public const byte CSIZET_SIZE = 8; // size_t 数据类型内存占用大小，一个字节
        public const byte INSTRUCTION_SIZE = 4; // 指令数据类型内存占用大小，一个字节
        public const byte LUA_INTEGER_SIZE = 8; // Lua 整形类型内存占用大小，一个字节
        public const byte LUA_NUMBER_SIZE = 8; // Lua 浮点类型内存占用大小，一个字节
        public const int LUAC_INT = 0x5678; // n 个字节，n 等于 Lua 整形占用的大小，存放整数 0x5678，用于确定机器大小端
        public const double LUAC_NUM = 370.5; // n 个字节，n 等于 Lua 浮点数占用的大小，存放浮点数 370.5，用于检测浮点数格式是否匹配
    }

    /// <summary>
    /// Lua 块中常量类型枚举
    /// </summary>
    enum EChunkTag : byte
    {
        Nil = 0x00,
        Boolean = 0x01,
        Number = 0x03,
        Integer = 0x13,
        ShortStr = 0x04,
        LongStr = 0x14,
    }

    /// <summary>
    /// Lua 预编译二进制块结构
    /// </summary>
    class BinaryChunk
    {
        public Header Header; // 头部
        public byte SizeUpvalues; // Upvalue 的数量
        public ProtoType MainFunc; // 主函数
    }

    /// <summary>
    /// Lua 二进制预编译块头部结构
    /// </summary>
    struct Header
    {
        public byte[] Signature; // Lua 签名，4个字节,用于校验读取的 Chunk 是否合法
        public byte Version; // 版本号，1个字节，由三部分组成，大版本号（Major Version）、小版本号（Minor Version）、发布号（Release Version），值为大版本号乘以 16 加小版本号
        public byte Format; // 格式号，1个字节，校验用，Lua 官方实现使用的格式号是 0
        public byte[] LuacData; // 6个字节，校验用，前两个字节为 0x1993，后面字节依次是 0x0d（回车）、0x0a（换行）、0x1a（替换符）、0x0a（换行符）
        public byte CIntSize; // c int 数据类型内存占用大小，一个字节
        public byte SizetSize; // size_t 数据类型内存占用大小，一个字节
        public byte InstructionSize; // 指令数据类型内存占用大小，一个字节
        public byte LuaIntegerSize; // Lua 整形类型内存占用大小，一个字节
        public byte LuaNumberSize; // Lua 浮点类型内存占用大小，一个字节
        public LuaInt LuacInt; // n 个字节，n 等于 Lua 整形占用的大小，存放整数 0x5678，用于确定机器大
        public LuaFloat LuacNum; // n 个字节，n 等于 Lua 浮点数占用的大小，存放浮点数 370.5，用于检测浮点数格式是否匹配
    }

    /// <summary>
    /// 函数原型结构
    /// </summary>
    class ProtoType
    {
        public string Source; // 源文件名
        public UInt32 LineDefined; // 起始行号
        public UInt32 LastLineDefined; // 终止行号
        public byte NumParams; // 固定参数个数
        public byte IsVararg; // 是否为变长函数
        public byte MaxStackSize; // 最大寄存器数量
        public UInt32[] Code; // 指令表
        public object[] Constatns; // 常量表
        public Upvalue[] Upvalues; // upvalue 表
        public ProtoType[] Protos; // 子函数原型表

        // --- 调试信息 ---
        public UInt32[] LineInfo; // 行号表
        public LocVar[] LocVars; // 局部变量表
        public string[] UpvalueNames; // Upvalue 名列表

        public static bool IsBinaryChunk(byte[] data)
        {
            if (data.Length <= 4)
                return false;

            var sign = new byte[4];
            Array.Copy(data, sign, 4);
            return sign.ToStr() == ChunkConst.LUA_SIGNATURE;
        }

        public static ProtoType Undump(byte[] data)
        {
            var reader = new Reader(data);
            if (reader.CheckHeader())
            {
                reader.ReadByte(); // size_upvalues
                return reader.ReadProto("");
            }

            return null;
        }
    }

    /// <summary>
    /// Upvalue 结构
    /// </summary>
    struct Upvalue
    {
        public byte Instack; // 标记是否在函数堆栈中
        public byte Idx; // upvalue 的索引（函数堆栈或外层upvalue索引）
    }

    /// <summary>
    /// 局部变量结构
    /// </summary>
    struct LocVar
    {
        public string VarName; // 变量名
        public UInt32 StartPC; // 起始指令索引
        public UInt32 EndPC; // 终止指令索引
    }
}