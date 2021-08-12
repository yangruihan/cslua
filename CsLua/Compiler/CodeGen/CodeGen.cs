using System.Collections.Generic;
using CsLua.Binchunk;
using CsLua.Compiler.Ast;

namespace CsLua.Compiler.CodeGen
{
    internal static partial class CodeGen
    {
        public static ProtoType GenProto(Block chunk)
        {
            var fd = new FuncDefExp
            {
                IsVararg = true,
                Block = chunk,
                ParList = new List<string>()
            };
            var fi = new FuncInfo(null, fd);
            fi.AddLocVar("_ENV");
            CgFuncDefExp(fi, fd, 0);
            return ToProto(fi.SubFuncs[0]);
        }
    }
}