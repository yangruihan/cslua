using System;
using CsLua.Common;

namespace CsLua.Binchunk
{
    class Reader
    {
        private byte[] _data;
        private int _idx;

        public Reader(byte[] data)
        {
            _data = data;
            _idx = 0;
        }

        public byte ReadByte()
        {
            return _data[_idx++];
        }

        public byte[] ReadBytes(int n)
        {
            var ret = new byte[n];
            Array.Copy(_data, _idx, ret, 0, n);
            _idx += n;
            return ret;
        }

        public UInt32 ReadUint32()
        {
            var ret = BitConverter.ToUInt32(_data, _idx);
            _idx += 4;
            return ret;
        }

        public UInt64 ReadUint64()
        {
            var ret = BitConverter.ToUInt64(_data, _idx);
            _idx += 8;
            return ret;
        }

        public Int64 ReadLuaInteger()
        {
            return Convert.ToInt64(ReadUint64());
        }

        public double ReadLuaNumber()
        {
            var ret = BitConverter.ToDouble(_data, _idx);
            _idx += 8;
            return ret;
        }

        public string ReadString()
        {
            var size = (uint) ReadByte();
            if (size == 0)
                return "";

            if (size == 0xff)
                size = (uint) ReadUint64();

            var bytes = ReadBytes((int) (size - 1));
            return bytes.ToStr();
        }

        public bool CheckHeader()
        {
            if (ReadBytes(4).ToStr() != ChunkConst.LUA_SIGNATURE)
                Debug.Error("not a precompiled chunk!");

            if (ReadByte() != ChunkConst.LUAC_VERSION)
                Debug.Error("version mismatch!");

            if (ReadByte() != ChunkConst.LUAC_FORMAT)
                Debug.Error("format mismatch!");

            if (ReadBytes(6).ToStr() != ChunkConst.LUAC_DATA)
                Debug.Error("corrupted!");

            if (ReadByte() != ChunkConst.CINT_SIZE)
                Debug.Error("int size mismatch!");

            if (ReadByte() != ChunkConst.CSIZET_SIZE)
                Debug.Error("size_t size mismatch!");

            if (ReadByte() != ChunkConst.INSTRUCTION_SIZE)
                Debug.Error("instruction size mismatch!");

            if (ReadByte() != ChunkConst.LUA_INTEGER_SIZE)
                Debug.Error("lua_Integer size mismatch!");

            if (ReadByte() != ChunkConst.LUA_NUMBER_SIZE)
                Debug.Error("lua_Number size mismatch!");

            if (ReadLuaInteger() != ChunkConst.LUAC_INT)
                Debug.Error("endianness mismatch!");

            if (ReadLuaNumber() != ChunkConst.LUAC_NUM)
                Debug.Error("float format mismatch!");

            return true;
        }

        public ProtoType ReadProto(string parentSource)
        {
            var source = ReadString();
            if (string.IsNullOrEmpty(source))
                source = parentSource;

            return new ProtoType
            {
                Source = source,
                LineDefined = ReadUint32(),
                LastLineDefined = ReadUint32(),
                NumParams = ReadByte(),
                IsVararg = ReadByte(),
                MaxStackSize = ReadByte(),
                Code = ReadCode(),
                Constatns = ReadConstants(),
                Upvalues = ReadUpvalues(),
                Protos = ReadProtos(source),
                LineInfo = ReadLineInfo(),
                LocVars = ReadLocVars(),
                UpvalueNames = ReadUpvalueNames()
            };
        }

        private UInt32[] ReadCode()
        {
            var code = new UInt32[ReadUint32()];
            for (var i = 0; i < code.Length; i++)
                code[i] = ReadUint32();
            return code;
        }

        private object[] ReadConstants()
        {
            var constants = new object[ReadUint32()];
            for (var i = 0; i < constants.Length; i++)
                constants[i] = ReadConstant();
            return constants;
        }

        private object ReadConstant()
        {
            switch ((EChunkTag) ReadByte())
            {
                case EChunkTag.Nil:
                    return null;

                case EChunkTag.Boolean:
                    return ReadByte() != 0;

                case EChunkTag.Integer:
                    return ReadLuaInteger();

                case EChunkTag.Number:
                    return ReadLuaNumber();

                case EChunkTag.ShortStr:
                case EChunkTag.LongStr:
                    return ReadString();

                default:
                    // todo
                    Debug.Error("corrupted!");
                    break;
            }

            return null;
        }

        private Upvalue[] ReadUpvalues()
        {
            var upvalues = new Upvalue[ReadUint32()];
            for (var i = 0; i < upvalues.Length; i++)
                upvalues[i] = new Upvalue
                {
                    Instack = ReadByte(),
                    Idx = ReadByte()
                };
            return upvalues;
        }

        private ProtoType[] ReadProtos(string parentSource)
        {
            var protos = new ProtoType[ReadUint32()];
            for (var i = 0; i < protos.Length; i++)
                protos[i] = ReadProto(parentSource);
            return protos;
        }

        private UInt32[] ReadLineInfo()
        {
            var lineInfo = new UInt32[ReadUint32()];
            for (var i = 0; i < lineInfo.Length; i++)
                lineInfo[i] = ReadUint32();
            return lineInfo;
        }

        private LocVar[] ReadLocVars()
        {
            var locVars = new LocVar[ReadUint32()];
            for (var i = 0; i < locVars.Length; i++)
                locVars[i] = new LocVar
                {
                    VarName = ReadString(),
                    StartPC = ReadUint32(),
                    EndPC = ReadUint32()
                };
            return locVars;
        }

        private string[] ReadUpvalueNames()
        {
            var names = new string[ReadUint32()];
            for (var i = 0; i < names.Length; i++)
                names[i] = ReadString();
            return names;
        }
    }
}