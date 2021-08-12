using System.Collections.Generic;
using CsLua.Compiler.Ast;

namespace CsLua.Compiler.CodeGen
{
    internal static partial class CodeGen
    {
        private static bool IsVarargOrFuncCall(Exp exp)
        {
            return exp is VarargExp || exp is FuncCallExp;
        }

        private static List<Exp> RemoveTailNil(List<Exp> exps)
        {
            if (exps == null)
                return new List<Exp>();

            for (var i = exps.Count - 1; i >= 0; i--)
            {
                if (!(exps[i] is NilExp))
                    return exps.GetRange(0, i + 1);
            }

            return null;
        }
    }
}