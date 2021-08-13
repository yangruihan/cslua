using System.Collections.Generic;
using CsLua.Misc;
using CsLua.Compiler.Ast;

namespace CsLua.Compiler.CodeGen
{
    internal static partial class CodeGen
    {
        private static void CgStat(FuncInfo fi, Stat node)
        {
            switch (node)
            {
                case FuncCallStat fs:
                    CgFuncCallStat(fi, fs);
                    break;

                case BreakStat bs:
                    CgBreakStat(fi, bs);
                    break;

                case DoStat ds:
                    CgDoStat(fi, ds);
                    break;

                case WhileStat ws:
                    CgWhileStat(fi, ws);
                    break;

                case RepeatStat rs:
                    CgRepeatStat(fi, rs);
                    break;

                case IfStat ifs:
                    CgIfStat(fi, ifs);
                    break;

                case ForNumStat fns:
                    CgForNumStat(fi, fns);
                    break;

                case ForInStat fis:
                    CgForInStat(fi, fis);
                    break;

                case AssignStat ass:
                    CgAssignStat(fi, ass);
                    break;

                case LocalVarDeclStat lvds:
                    CgLocalVarDeclStat(fi, lvds);
                    break;

                case LocalFuncDefStat lfds:
                    CgLocalFuncDefStat(fi, lfds);
                    break;

                case LabelStat _:
                case GotoStat _:
                    Debug.Panic("label and goto statements are not supported!");
                    break;
            }
        }

        private static void CgLocalFuncDefStat(FuncInfo fi, LocalFuncDefStat node)
        {
            var r = fi.AddLocVar(node.Name);
            CgFuncDefExp(fi, node.Exp, r);
        }

        private static void CgFuncCallStat(FuncInfo fi, FuncCallStat node)
        {
            var r = fi.AllocReg();
            CgFuncCallExp(fi, node, r, 0);
            fi.FreeReg();
        }

        private static void CgBreakStat(FuncInfo fi, BreakStat node)
        {
            var pc = fi.EmitJmp(0, 0);
            fi.AddBreakJump(pc);
        }

        private static void CgDoStat(FuncInfo fi, DoStat node)
        {
            fi.EnterScope(false);
            CgBlock(fi, node.Block);
            fi.CloseOpenUpvals();
            fi.ExitScope();
        }

        private static void CgWhileStat(FuncInfo fi, WhileStat node)
        {
            var pcBeforeExp = fi.PC();

            var r = fi.AllocReg();
            CgExp(fi, node.Exp, r, 1);
            fi.FreeReg();

            fi.EmitTest(r, 0);
            var pcJmpToEnd = fi.EmitJmp(0, 0);

            fi.EnterScope(true);
            CgBlock(fi, node.Block);
            fi.CloseOpenUpvals();
            fi.EmitJmp(0, pcBeforeExp - fi.PC() - 1);
            fi.ExitScope();

            fi.FixsBx(pcJmpToEnd, fi.PC() - pcJmpToEnd);
        }

        private static void CgRepeatStat(FuncInfo fi, RepeatStat node)
        {
            fi.EnterScope(true);

            var pcBeforeBlock = fi.PC();
            CgBlock(fi, node.Block);

            var r = fi.AllocReg();
            CgExp(fi, node.Exp, r, 1);
            fi.FreeReg();

            fi.EmitTest(r, 0);
            fi.EmitJmp(fi.GetJmpArgA(), pcBeforeBlock - fi.PC() - 1);
            fi.CloseOpenUpvals();

            fi.ExitScope();
        }

        private static void CgIfStat(FuncInfo fi, IfStat node)
        {
            var pcJmpToEnds = new int[node.Exps.Count];
            var pcJmpToNextExp = -1;

            for (var i = 0; i < node.Exps.Count; i++)
            {
                var exp = node.Exps[i];

                if (pcJmpToNextExp >= 0)
                    fi.FixsBx(pcJmpToNextExp, fi.PC() - pcJmpToNextExp);

                var r = fi.AllocReg();
                CgExp(fi, exp, r, 1);
                fi.FreeReg();

                fi.EmitTest(r, 0);
                pcJmpToNextExp = fi.EmitJmp(0, 0);

                fi.EnterScope(false);
                CgBlock(fi, node.Blocks[i]);
                fi.CloseOpenUpvals();
                fi.ExitScope();

                if (i < node.Exps.Count - 1)
                    pcJmpToEnds[i] = fi.EmitJmp(0, 0);
                else
                    pcJmpToEnds[i] = pcJmpToNextExp;
            }

            foreach (var pc in pcJmpToEnds)
                fi.FixsBx(pc, fi.PC() - pc);
        }

        private static void CgForNumStat(FuncInfo fi, ForNumStat node)
        {
            fi.EnterScope(true);

            CgLocalVarDeclStat(fi, new LocalVarDeclStat
            {
                NameList = new List<string> {"(for index)", "(for limit)", "(for step)"},
                ExpList = new List<Exp> {node.InitExp, node.LimitExp, node.StepExp}
            });
            fi.AddLocVar(node.VarName);

            var a = fi.UsedRegs - 4;
            var pcForPrep = fi.EmitForPrep(a, 0);
            CgBlock(fi, node.Block);
            fi.CloseOpenUpvals();
            var pcForLoop = fi.EmitForLoop(a, 0);

            fi.FixsBx(pcForPrep, pcForLoop - pcForPrep - 1);
            fi.FixsBx(pcForLoop, pcForPrep - pcForLoop);

            fi.ExitScope();
        }

        private static void CgForInStat(FuncInfo fi, ForInStat node)
        {
            fi.EnterScope(true);

            CgLocalVarDeclStat(fi, new LocalVarDeclStat
            {
                NameList = new List<string> {"(for generator)", "(for state)", "(for control)"},
                ExpList = node.ExpList
            });

            foreach (var name in node.NameList)
                fi.AddLocVar(name);

            var pcJmpToTfc = fi.EmitJmp(0, 0);
            CgBlock(fi, node.Block);
            fi.CloseOpenUpvals();
            fi.FixsBx(pcJmpToTfc, fi.PC() - pcJmpToTfc);

            var rGenerator = fi.SlotOfLocVar("(for generator)");
            fi.EmitTForCall(rGenerator, node.NameList.Count);
            fi.EmitTForLoop(rGenerator + 2, pcJmpToTfc - fi.PC() - 1);
            fi.ExitScope();
        }

        private static void CgLocalVarDeclStat(FuncInfo fi, LocalVarDeclStat node)
        {
            var exps = RemoveTailNil(node.ExpList);
            var nExps = exps.Count;
            var nNames = node.NameList.Count;
            var oldRegs = fi.UsedRegs;

            if (nExps == nNames)
            {
                foreach (var exp in exps)
                {
                    var a = fi.AllocReg();
                    CgExp(fi, exp, a, 1);
                }
            }
            else if (nExps > nNames)
            {
                for (var i = 0; i < exps.Count; i++)
                {
                    var exp = exps[i];
                    var a = fi.AllocReg();
                    if (i == nExps - 1 && IsVarargOrFuncCall(exp))
                        CgExp(fi, exp, a, 0);
                    else
                        CgExp(fi, exp, a, 1);
                }
            }
            else
            {
                var multRet = false;
                for (var i = 0; i < exps.Count; i++)
                {
                    var exp = exps[i];
                    var a = fi.AllocReg();
                    if (i == nExps - 1 && IsVarargOrFuncCall(exp))
                    {
                        multRet = true;
                        var n = nNames - nExps + 1;
                        CgExp(fi, exp, a, n);
                        fi.AllocRegs(n - 1);
                    }
                    else
                    {
                        CgExp(fi, exp, a, 1);
                    }
                }

                if (!multRet)
                {
                    var n = nNames - nExps;
                    var a = fi.AllocRegs(n);
                    fi.EmitLoadNil(a, n);
                }
            }

            fi.UsedRegs = oldRegs;
            foreach (var name in node.NameList)
                fi.AddLocVar(name);
        }

        private static void CgAssignStat(FuncInfo fi, AssignStat node)
        {
            var exps = RemoveTailNil(node.ExpList);
            var nExps = exps.Count;
            var nVars = node.VarList.Count;

            var tRegs = new int[nVars];
            var kRegs = new int[nVars];
            var vRegs = new int[nVars];
            var oldRegs = fi.UsedRegs;

            for (var i = 0; i < node.VarList.Count; i++)
            {
                var exp = node.VarList[i];

                if (exp is TableAccessExp tableAccessExp)
                {
                    tRegs[i] = fi.AllocReg();
                    CgExp(fi, tableAccessExp.PrefixExp, tRegs[i], 1);
                    kRegs[i] = fi.AllocReg();
                    CgExp(fi, tableAccessExp.KeyExp, kRegs[i], 1);
                }
                else if (exp is NameExp nameExp)
                {
                    var name = nameExp.Name;

                    if (fi.SlotOfLocVar(name) < 0 && fi.IndexOfUpval(name) < 0)
                    {
                        kRegs[i] = -1;
                        fi.IndexOfConstant(name, out var constIdx);
                        if (constIdx > 0xff)
                        {
                            kRegs[i] = fi.AllocReg();
                        }
                    }
                }
            }

            for (var i = 0; i < nVars; i++)
                vRegs[i] = fi.UsedRegs + i;

            if (nExps >= nVars)
            {
                for (var i = 0; i < exps.Count; i++)
                {
                    var exp = exps[i];
                    var a = fi.AllocReg();
                    if (i >= nVars && i == nExps - 1 && IsVarargOrFuncCall(exp))
                        CgExp(fi, exp, a, 0);
                    else
                        CgExp(fi, exp, a, 1);
                }
            }
            else
            {
                var multRet = false;

                for (var i = 0; i < exps.Count; i++)
                {
                    var exp = exps[i];
                    var a = fi.AllocReg();
                    if (i == nExps - 1 && IsVarargOrFuncCall(exp))
                    {
                        multRet = true;
                        var n = nVars - nExps + 1;
                        CgExp(fi, exp, a, n);
                        fi.AllocRegs(n - 1);
                    }
                    else
                    {
                        CgExp(fi, exp, a, 1);
                    }
                }

                if (!multRet)
                {
                    var n = nVars - nExps;
                    var a = fi.AllocRegs(n);
                    fi.EmitLoadNil(a, n);
                }
            }

            for (var i = 0; i < node.VarList.Count; i++)
            {
                var exp = node.VarList[i];

                if (exp is NameExp nameExp)
                {
                    var varName = nameExp.Name;
                    var a = fi.SlotOfLocVar(varName);

                    if (a >= 0)
                    {
                        fi.EmitMove(a, vRegs[i]);
                    }
                    else
                    {
                        var b = fi.IndexOfUpval(varName);
                        if (b >= 0)
                        {
                            fi.EmitSetUpval(vRegs[i], b);
                        }
                        else
                        {
                            a = fi.SlotOfLocVar("_ENV");
                            if (a >= 0)
                            {
                                if (kRegs[i] < 0)
                                {
                                    fi.IndexOfConstant(varName, out var constIdx);
                                    b = 0x100 + constIdx;
                                    fi.EmitSetTable(a, b, vRegs[i]);
                                }
                                else
                                {
                                    fi.EmitSetTable(a, kRegs[i], vRegs[i]);
                                }
                            }
                            else
                            {
                                a = fi.IndexOfUpval("_ENV");
                                if (kRegs[i] < 0)
                                {
                                    fi.IndexOfConstant(varName, out var idx);
                                    b = 0x100 + idx;
                                    fi.EmitSetTabUp(a, b, vRegs[i]);
                                }
                                else
                                {
                                    fi.EmitSetTable(a, kRegs[i], vRegs[i]);
                                }
                            }
                        }
                    }
                }
                else
                {
                    fi.EmitSetTable(tRegs[i], kRegs[i], vRegs[i]);
                }
            }

            fi.UsedRegs = oldRegs;
        }
    }
}