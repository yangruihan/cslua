using System.Collections.Generic;
using CsLua.Compiler.Ast;

namespace CsLua.Compiler.CodeGen
{
    partial class CodeGen
    {
        private static void CgBlock(FuncInfo fi, Block node)
        {
            foreach (var stat in node.Stats)
            {
                CgStat(fi, stat);
            }

            if (node.RetExps != null)
            {
                CgRetStat(fi, node.RetExps);
            }
        }

        private static void CgRetStat(FuncInfo fi, List<Exp> exps)
        {
            var nExps = exps.Count;
            if (nExps == 0)
            {
                fi.EmitReturn(0, 0);
                return;
            }

            if (nExps == 1)
            {
                if (exps[0] is NameExp nameExp)
                {
                    var r = fi.SlotOfLocVar(nameExp.Name);
                    if (r >= 0)
                    {
                        fi.EmitReturn(r, 1);
                        return;
                    }
                }

                if (exps[0] is FuncCallExp fcExp)
                {
                    var r = fi.AllocReg();
                    CgTailCallExp(fi, fcExp, r);
                    fi.FreeReg();
                    fi.EmitReturn(r, -1);
                    return;
                }
            }

            var multRet = IsVarargOrFuncCall(exps[nExps - 1]);

            for (var i = 0; i < nExps; i++)
            {
                var exp = exps[i];
                var r = fi.AllocReg();

                if (i == nExps - 1 && multRet)
                    CgExp(fi, exp, r, -1);
                else
                    CgExp(fi, exp, r, 1);
            }

            fi.FreeRegs(nExps);

            var a = fi.UsedRegs;
            if (multRet)
                fi.EmitReturn(a, -1);
            else
                fi.EmitReturn(a, nExps);
        }
    }
}