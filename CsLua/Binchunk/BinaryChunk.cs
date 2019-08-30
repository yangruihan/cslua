using System;

namespace CsLua.Binchunk
{
    public class ChunkConst
    {
        public const string LUA_SIGNATURE = "\u001bLua";
        public const byte LUAC_VERSION = 0x53;
        public const byte LUAC_FORMAT = 0;
        public const string LUAC_DATA = "\u0019\u0093\u000d\u000a\u001a\u000a";
        public const byte CINT_SIZE = 4;
        public const byte CSIZET_SIZE = 8;
        public const byte INSTRUCTION_SIZE = 4;
        public const byte LUA_INTEGER_SIZE = 8;
        public const byte LUA_NUMBER_SIZE = 8;
        public const int LUAC_INT = 0x5678;
        public const double LUAC_NUM = 370.5;
    }

    enum ChunkTag : byte
    {
        Nil = 0x00,
        Boolean = 0x01,
        Number = 0x03,
        Integer = 0x13,
        ShortStr = 0x04,
        LongStr = 0x14,
    }

    class BinaryChunk
    {
        public Header Header;
        public byte SizeUpvalues;
        public ProtoType MainFunc;
    }

    struct Header
    {
        public byte[] Signature;
        public byte Version;
        public byte Format;
        public byte[] LuacData;
        public byte CIntSize;
        public byte SizetSize;
        public byte InstructionSize;
        public byte LuaIntegerSize;
        public byte LuaNumberSize;
        public Int64 LuacInt;
        public double LuacNum;
    }

    class ProtoType
    {
        public string Source;
        public UInt32 LineDefined;
        public UInt32 LastLineDefined;
        public byte NumParams;
        public byte IsVararg;
        public byte MaxStackSize;
        public UInt32[] Code;
        public object[] Constatns;
        public Upvalue[] Upvalues;
        public ProtoType[] Protos;

        // --- debug info ---
        public UInt32[] LineInfo;
        public LocVar[] LocVars;
        public string[] UpvalueNames;

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

    struct Upvalue
    {
        public byte Instack;
        public byte Idx;
    }

    struct LocVar
    {
        public string VarName;
        public UInt32 StartPC;
        public UInt32 EndPC;
    }
}