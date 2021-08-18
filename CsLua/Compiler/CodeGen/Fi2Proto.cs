using System.Collections.Generic;
using CsLua.Binchunk;
using CsLua.VM;

namespace CsLua.Compiler.CodeGen
{
    internal static partial class CodeGen
    {
        private static ProtoType ToProto(FuncInfo fi)
        {
            var proto = new ProtoType
            {
                NumParams = (byte)fi.NumParams,
                MaxStackSize = (byte)fi.MaxRegs,
                Code = Instruction.FromIntArr(fi.Insts.ToArray()),
                Constatns = GetConstants(fi),
                Upvalues = GetUpvalues(fi),
                Protos = ToProtos(fi.SubFuncs),
                LineInfo = new uint[0],
                LocVars = new LocVar[0],
                UpvalueNames = new string[0]
            };

            if (proto.MaxStackSize < 2)
                proto.MaxStackSize = 2;

            if (fi.IsVararg)
                proto.IsVararg = 1;

            return proto;
        }

        private static ProtoType[] ToProtos(List<FuncInfo> fis)
        {
            var protos = new ProtoType[fis.Count];
            for (var i = 0; i < fis.Count; i++)
                protos[i] = ToProto(fis[i]);

            return protos;
        }

        private static object[] GetConstants(FuncInfo fi)
        {
            var consts = new object[fi.Constants.Count];
            foreach (var kv in fi.Constants)
            {
                if (kv.Key == FuncInfo.NilObj)
                    consts[kv.Value] = null;
                else
                    consts[kv.Value] = kv.Key;
            }

            return consts;
        }

        private static Upvalue[] GetUpvalues(FuncInfo fi)
        {
            var upvals = new Upvalue[fi.Upvalues.Count];
            foreach (var kv in fi.Upvalues)
            {
                if (kv.Value.LocVarSlot >= 0)
                {
                    upvals[kv.Value.Index] = new Upvalue
                    {
                        Instack = 1,
                        Idx = (byte)kv.Value.LocVarSlot
                    };
                }
                else
                {
                    upvals[kv.Value.Index] = new Upvalue
                    {
                        Instack = 0,
                        Idx = (byte)kv.Value.UpvalIndex
                    };
                }
            }

            return upvals;
        }
    }
}