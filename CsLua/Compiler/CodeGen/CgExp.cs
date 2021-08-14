using System.Linq;
using CsLua.API;
using CsLua.Misc;
using CsLua.Compiler.Ast;
using CsLua.Compiler.Lexer;
using CsLua.VM;

namespace CsLua.Compiler.CodeGen
{
    internal static partial class CodeGen
    {
        private static void CgExp(FuncInfo fi, Exp node, int a, int n)
        {
            switch (node)
            {
                case NilExp _:
                    fi.EmitLoadNil(a, n);
                    break;

                case FalseExp _:
                    fi.EmitLoadBool(a, 0, 0);
                    break;

                case TrueExp _:
                    fi.EmitLoadBool(a, 1, 0);
                    break;

                case IntegerExp integerExp:
                    fi.EmitLoadK(a, integerExp.Val);
                    break;

                case FloatExp floatExp:
                    fi.EmitLoadK(a, floatExp.Val);
                    break;

                case StringExp stringExp:
                    fi.EmitLoadK(a, stringExp.Str);
                    break;

                case ParensExp parensExp:
                    CgExp(fi, parensExp.Exp, a, 1);
                    break;

                case VarargExp varargExp:
                    CgVarargExp(fi, varargExp, a, n);
                    break;

                case FuncDefExp funcDefExp:
                    CgFuncDefExp(fi, funcDefExp, a);
                    break;

                case TableConstructorExp tableConstructorExp:
                    CgTableConstructorExp(fi, tableConstructorExp, a);
                    break;

                case UnopExp unopExp:
                    CgUnopExp(fi, unopExp, a);
                    break;

                case BinopExp binopExp:
                    CgBinopExp(fi, binopExp, a);
                    break;

                case ConcatExp concatExp:
                    CgConcatExp(fi, concatExp, a);
                    break;

                case NameExp nameExp:
                    CgNameExp(fi, nameExp, a);
                    break;

                case TableAccessExp tableAccessExp:
                    CgTableAccessExp(fi, tableAccessExp, a);
                    break;

                case FuncCallExp funcCallExp:
                    CgFuncCallExp(fi, funcCallExp, a, n);
                    break;
            }
        }

        private static void CgVarargExp(FuncInfo fi, VarargExp node, int a, int n)
        {
            if (!fi.IsVararg)
            {
                Debug.Panic("cannot use '...' outside a vararg function", EStatus.ErrSyntax);
                return;
            }

            fi.EmitVararg(a, n);
        }

        private static void CgFuncDefExp(FuncInfo fi, FuncDefExp node, int a)
        {
            var subFi = new FuncInfo(fi, node);
            fi.SubFuncs.Add(subFi);

            foreach (var param in node.ParList)
                subFi.AddLocVar(param);

            CgBlock(subFi, node.Block);
            subFi.ExitScope();
            subFi.EmitReturn(0, 0);
            var bx = fi.SubFuncs.Count - 1;
            fi.EmitClosure(a, bx);
        }

        private static void CgTableConstructorExp(FuncInfo fi, TableConstructorExp node, int a)
        {
            var nArr = node.KeyExps?.Count(exp => exp is null) ?? 0;
            var nExps = node.KeyExps?.Count ?? 0;
            var multRet = nExps > 0 && IsVarargOrFuncCall(node.ValExps[nExps - 1]);
            fi.EmitNewTable(a, nArr, nExps - nArr);
            var arrIdx = 0;

            for (var i = 0; i < nExps; i++)
            {
                var keyExp = node.KeyExps[i];
                var valExp = node.ValExps[i];

                if (keyExp == null)
                {
                    arrIdx++;
                    var tmp = fi.AllocReg();
                    if (i == nExps - 1 && multRet)
                        CgExp(fi, valExp, tmp, -1);
                    else
                        CgExp(fi, valExp, tmp, 1);

                    if (arrIdx % 50 == 0 || arrIdx == nArr)
                    {
                        var n = arrIdx % 50;
                        if (n == 0)
                            n = 50;

                        fi.FreeRegs(n);
                        var c = (arrIdx - 1) / 50 + 1;
                        if (i == nExps - 1 && multRet)
                            fi.EmitSetList(a, 0, c);
                        else
                            fi.EmitSetList(a, n, c);
                    }

                    continue;
                }

                var b = fi.AllocReg();
                CgExp(fi, keyExp, b, 1);
                var d = fi.AllocReg();
                CgExp(fi, valExp, d, 1);
                fi.FreeRegs(2);
                fi.EmitSetTable(a, b, d);
            }
        }

        private static void CgUnopExp(FuncInfo fi, UnopExp node, int a)
        {
            var b = fi.AllocReg();
            CgExp(fi, node.Exp, b, 1);
            fi.EmitUnaryOp(node.GetOpCode(), a, b);
            fi.FreeReg();
        }


        private static void CgBinopExp(FuncInfo fi, BinopExp node, int a)
        {
            switch (node.Op)
            {
                case ETokenType.OpAnd:
                case ETokenType.OpOr:
                {
                    var b = fi.AllocReg();
                    CgExp(fi, node.Exp1, b, 1);
                    fi.FreeReg();

                    if (node.Op == ETokenType.OpAnd)
                        fi.EmitTestSet(a, b, 0);
                    else
                        fi.EmitTestSet(a, b, 1);

                    var pcOfJmp = fi.EmitJmp(0, 0);
                    b = fi.AllocReg();
                    CgExp(fi, node.Exp2, b, 1);
                    fi.FreeReg();
                    fi.EmitMove(a, b);
                    fi.FixsBx(pcOfJmp, fi.PC() - pcOfJmp);

                    break;
                }

                default:
                {
                    var b = fi.AllocReg();
                    CgExp(fi, node.Exp1, b, 1);
                    var c = fi.AllocReg();
                    CgExp(fi, node.Exp2, c, 1);
                    fi.EmitBinaryOp(node.Op, a, b, c);
                    fi.FreeRegs(2);
                    break;
                }
            }
        }

        private static void CgConcatExp(FuncInfo fi, ConcatExp node, int a)
        {
            foreach (var subExp in node.Exps)
            {
                var tmp = fi.AllocReg();
                CgExp(fi, subExp, tmp, 1);
            }

            var c = fi.UsedRegs - 1;
            var b = c - node.Exps.Count + 1;
            fi.FreeRegs(c - b + 1);
            fi.EmitABC(EOpCode.OP_CONCAT, a, b, c);
        }

        private static void CgNameExp(FuncInfo fi, NameExp node, int a)
        {
            var r = fi.SlotOfLocVar(node.Name);

            if (r >= 0)
            {
                fi.EmitMove(a, r);
            }
            else
            {
                var idx = fi.IndexOfUpval(node.Name);
                if (idx >= 0)
                {
                    fi.EmitGetUpval(a, idx);
                }
                else
                {
                    var taExp = new TableAccessExp
                    {
                        PrefixExp = new NameExp
                        {
                            Line = 0,
                            Name = "_ENV"
                        },
                        KeyExp = new StringExp
                        {
                            Line = 0,
                            Str = node.Name
                        }
                    };
                    CgTableAccessExp(fi, taExp, a);
                }
            }
        }

        private static void CgTableAccessExp(FuncInfo fi, TableAccessExp node, int a)
        {
            var b = fi.AllocReg();
            CgExp(fi, node.PrefixExp, b, 1);
            var c = fi.AllocReg();
            CgExp(fi, node.KeyExp, c, 1);
            fi.EmitGetTable(a, b, c);
            fi.FreeRegs(2);
        }

        private static void CgFuncCallExp(FuncInfo fi, FuncCallExp node, int a, int n)
        {
            var nArgs = PrepFuncCall(fi, node, a);
            fi.EmitCall(a, nArgs, n);
        }

        private static void CgTailCallExp(FuncInfo fi, FuncCallExp node, int a)
        {
            var nArgs = PrepFuncCall(fi, node, a);
            fi.EmitTailCall(a, nArgs);
        }

        private static int PrepFuncCall(FuncInfo fi, FuncCallExp node, int a)
        {
            var nArgs = node.Args?.Count ?? 0;
            var lastArgIsVarargsOrFuncCall = false;

            CgExp(fi, node.PrefixExp, a, 1);
            if (node.NameExp != null)
            {
                fi.IndexOfConstant(node.NameExp.Str, out var idx);
                var c = 0x100 + idx;
                fi.EmitSelf(a, a, c);
            }

            for (var i = 0; i < nArgs; i++)
            {
                var arg = node.Args[i];
                var tmp = fi.AllocReg();
                if (i == nArgs - 1 && IsVarargOrFuncCall(arg))
                {
                    lastArgIsVarargsOrFuncCall = true;
                    CgExp(fi, arg, tmp, -1);
                }
                else
                {
                    CgExp(fi, arg, tmp, 1);
                }
            }

            fi.FreeRegs(nArgs);

            if (node.NameExp != null)
                nArgs++;

            if (lastArgIsVarargsOrFuncCall)
                nArgs = -1;

            return nArgs;
        }
    }
}