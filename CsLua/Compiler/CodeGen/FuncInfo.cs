using System;
using System.Collections.Generic;
using CsLua.Common;
using CsLua.Compiler.Ast;
using CsLua.Compiler.Lexer;
using CsLua.VM;

namespace CsLua.Compiler.CodeGen
{
    struct UpvalInfo
    {
        public int LocVarSlot;
        public int UpvalIndex;
        public int Index;
    }

    class LocVarInfo
    {
        public LocVarInfo Prev;
        public string Name;
        public int ScopeLv;
        public int Slot;
        public bool Captured;
    }

    class FuncInfo
    {
        public static readonly object NilObj = new byte();

        private static readonly Dictionary<ETokenType, EOpCode> ArithAndBitwiseBinops =
            new Dictionary<ETokenType, EOpCode>
            {
                {ETokenType.OpAdd, EOpCode.OP_ADD},
                {ETokenType.OpSub, EOpCode.OP_SUB},
                {ETokenType.OpMul, EOpCode.OP_MUL},
                {ETokenType.OpMod, EOpCode.OP_MOD},
                {ETokenType.OpPow, EOpCode.OP_POW},
                {ETokenType.OpDiv, EOpCode.OP_DIV},
                {ETokenType.OpIDiv, EOpCode.OP_IDIV},
                {ETokenType.OpBAnd, EOpCode.OP_BAND},
                {ETokenType.OpBOr, EOpCode.OP_BOR},
                {ETokenType.OpBXor, EOpCode.OP_BXOR},
                {ETokenType.OpShl, EOpCode.OP_SHL},
                {ETokenType.OpShr, EOpCode.OP_SHR}
            };

        private readonly FuncInfo _parent;

        public List<FuncInfo> SubFuncs { get; }

        public int UsedRegs { set; get; }

        public int MaxRegs { get; private set; }

        private int _scopeLv;

        private readonly List<LocVarInfo> _locVars;

        private readonly Dictionary<string, LocVarInfo> _locNames;

        public Dictionary<string, UpvalInfo> Upvalues { get; }

        public Dictionary<object, int> Constants { get; }

        private readonly List<List<int>> _breaks;

        public List<uint> Insts { get; }

        public int NumParams { get; }

        public bool IsVararg { get; }

        public FuncInfo(FuncInfo parent, FuncDefExp fd)
        {
            _parent = parent;
            SubFuncs = new List<FuncInfo>();
            _locVars = new List<LocVarInfo>(8);
            _locNames = new Dictionary<string, LocVarInfo>();
            Upvalues = new Dictionary<string, UpvalInfo>();
            Constants = new Dictionary<object, int>();
            _breaks = new List<List<int>>(1);
            Insts = new List<uint>(8);
            NumParams = fd.ParList.Count;
            IsVararg = fd.IsVararg;
        }

        public bool IndexOfConstant(object k, out int idx)
        {
            if (k is null)
                k = NilObj;

            if (Constants.ContainsKey(k))
            {
                idx = Constants[k];
                return true;
            }

            idx = Constants.Count;
            Constants[k] = idx;
            return true;
        }

        public int AllocReg()
        {
            UsedRegs++;
            if (UsedRegs >= 255)
            {
                Debug.Panic("function or expression needs too many registers");
                return -1;
            }

            if (UsedRegs > MaxRegs)
                MaxRegs = UsedRegs;

            return UsedRegs - 1;
        }

        public int AllocRegs(int n)
        {
            if (n <= 0)
            {
                Debug.Panic("n < 0!");
                return -1;
            }

            for (var i = 0; i < n; i++)
                AllocReg();

            return UsedRegs - n;
        }

        public void FreeReg()
        {
            if (UsedRegs <= 0)
            {
                Debug.Panic("UsedRegs <= 0!");
                return;
            }

            UsedRegs--;
        }

        public void FreeRegs(int n)
        {
            if (n < 0)
            {
                Debug.Panic("n < 0!");
                return;
            }

            for (var i = 0; i < n; i++)
                FreeReg();
        }

        public void EnterScope(bool breakable)
        {
            _scopeLv++;

            _breaks.Add(breakable ? new List<int>() : null);
        }

        public void ExitScope()
        {
            var pendingBreakJmpsIdx = _breaks.Count - 1;
            if (pendingBreakJmpsIdx >= 0)
            {
                var pendingBreakJmps = _breaks[pendingBreakJmpsIdx];
                _breaks.RemoveAt(pendingBreakJmpsIdx);

                if (pendingBreakJmps != null)
                {
                    var a = GetJmpArgA();

                    foreach (var pc in pendingBreakJmps)
                    {
                        var sBx = PC() - pc;
                        var i = (sBx + Instruction.MAXARG_sBx) << 14 | a << 6 | (int) EOpCode.OP_JMP;
                        Insts[pc] = (UInt32) i;
                    }
                }

                _scopeLv--;

                var waitToRemoveList = new List<LocVarInfo>();
                foreach (var locVar in _locNames)
                {
                    if (locVar.Value.ScopeLv > _scopeLv)
                        waitToRemoveList.Add(locVar.Value);
                }

                foreach (var i in waitToRemoveList)
                {
                    RemoveLocVar(i);
                }
            }
        }

        public void RemoveLocVar(LocVarInfo locVarInfo)
        {
            FreeReg();

            if (locVarInfo.Prev is null)
            {
                _locNames.Remove(locVarInfo.Name);
            }
            else if (locVarInfo.Prev.ScopeLv == locVarInfo.ScopeLv)
            {
                RemoveLocVar(locVarInfo.Prev);
            }
            else
            {
                _locNames[locVarInfo.Name] = locVarInfo.Prev;
            }
        }

        public int AddLocVar(string name)
        {
            var newVar = new LocVarInfo
            {
                Name = name,
                Prev = _locNames.ContainsKey(name) ? _locNames[name] : null,
                ScopeLv = _scopeLv,
                Slot = AllocReg()
            };

            _locVars.Add(newVar);
            _locNames[name] = newVar;
            return newVar.Slot;
        }

        public int GetJmpArgA()
        {
            var hasCapturedLocVars = false;
            var minSlotOfLocVars = MaxRegs;
            foreach (var locVar in _locNames)
            {
                if (locVar.Value.ScopeLv == _scopeLv)
                {
                    for (var v = locVar.Value; v != null && v.ScopeLv == _scopeLv; v = v.Prev)
                    {
                        if (v.Captured)
                        {
                            hasCapturedLocVars = true;
                        }

                        if (v.Slot < minSlotOfLocVars && v.Name[0] != '(')
                        {
                            minSlotOfLocVars = v.Slot;
                        }
                    }
                }
            }

            if (hasCapturedLocVars)
                return minSlotOfLocVars + 1;

            return 0;
        }

        public int SlotOfLocVar(string name)
        {
            if (_locNames.ContainsKey(name))
                return _locNames[name].Slot;

            return -1;
        }

        public void AddBreakJump(int pc)
        {
            for (var i = _scopeLv; i >= 0; i--)
            {
                if (_breaks[i] is null) continue;

                _breaks[i].Add(pc);
                return;
            }

            Debug.Panic("<break> at line ? not inside a loop!");
        }

        public int IndexOfUpval(string name)
        {
            if (Upvalues.ContainsKey(name))
                return Upvalues[name].Index;

            if (_parent != null)
            {
                if (_parent._locNames.TryGetValue(name, out var locVar))
                {
                    var idx = Upvalues.Count;
                    Upvalues[name] = new UpvalInfo
                    {
                        LocVarSlot = locVar.Slot,
                        UpvalIndex = -1,
                        Index = idx
                    };
                    locVar.Captured = true;
                    return idx;
                }

                var uvIdx = _parent.IndexOfUpval(name);
                if (uvIdx >= 0)
                {
                    var idx = Upvalues.Count;
                    Upvalues[name] = new UpvalInfo
                    {
                        LocVarSlot = -1,
                        UpvalIndex = uvIdx,
                        Index = idx
                    };
                    return idx;
                }
            }

            return -1;
        }

        public int PC()
        {
            return Insts.Count - 1;
        }

        public void FixsBx(int pc, int sBx)
        {
            var i = Insts[pc];
            i = i << 18 >> 18;
            i = i | (UInt32) (sBx + Instruction.MAXARG_sBx) << 14;
            Insts[pc] = i;
        }

        public void EmitABC(EOpCode opCode, int a, int b, int c)
        {
            var i = (uint)b << 23 | (uint)c << 14 | (uint)a << 6 | opCode.ToInt();
            Insts.Add((UInt32) i);
        }

        public void EmitABx(EOpCode opCode, int a, int bx)
        {
            var i = (uint)bx << 14 | (uint)a << 6 | opCode.ToInt();
            Insts.Add((UInt32) i);
        }

        public void EmitAsBx(EOpCode opCode, int a, int b)
        {
            var i = (uint)(b + Instruction.MAXARG_sBx) << 14 | (uint)a << 6 | opCode.ToInt();
            Insts.Add((UInt32) i);
        }

        public void EmitAx(EOpCode opCode, int ax)
        {
            var i = (uint)ax << 6 | opCode.ToInt();
            Insts.Add((UInt32) i);
        }

        public void EmitMove(int a, int b)
        {
            EmitABC(EOpCode.OP_MOVE, a, b, 0);
        }

        public void EmitLoadNil(int a, int n)
        {
            EmitABC(EOpCode.OP_LOADNIL, a, n - 1, 0);
        }

        public void EmitLoadBool(int a, int b, int c)
        {
            EmitABC(EOpCode.OP_LOADBOOL, a, b, c);
        }

        public void EmitLoadK(int a, object k)
        {
            if (IndexOfConstant(k, out var idx))
            {
                if (idx < (1 << 18))
                {
                    EmitABx(EOpCode.OP_LOADK, a, idx);
                }
                else
                {
                    EmitABx(EOpCode.OP_LOADK, a, 0);
                    EmitAx(EOpCode.OP_EXTRAARG, idx);
                }
            }
        }

        public void EmitVararg(int a, int n)
        {
            EmitABC(EOpCode.OP_VARARG, a, n + 1, 0);
        }

        public void EmitClosure(int a, int bx)
        {
            EmitABx(EOpCode.OP_CLOSURE, a, bx);
        }

        public void EmitNewTable(int a, int nArr, int nRec)
        {
            EmitABC(EOpCode.OP_NEWTABLE, a, nArr.ToFb(), nRec.ToFb());
        }

        public void EmitSetList(int a, int b, int c)
        {
            EmitABC(EOpCode.OP_SETLIST, a, b, c);
        }

        public void EmitGetTable(int a, int b, int c)
        {
            EmitABC(EOpCode.OP_GETTABLE, a, b, c);
        }

        public void EmitSetTable(int a, int b, int c)
        {
            EmitABC(EOpCode.OP_SETTABLE, a, b, c);
        }

        public void EmitGetUpval(int a, int b)
        {
            EmitABC(EOpCode.OP_GETUPVAL, a, b, 0);
        }

        public void EmitSetUpval(int a, int b)
        {
            EmitABC(EOpCode.OP_SETUPVAL, a, b, 0);
        }

        public void EmitGetTabUp(int a, int b, int c)
        {
            EmitABC(EOpCode.OP_GETTABUP, a, b, c);
        }

        public void EmitSetTabUp(int a, int b, int c)
        {
            EmitABC(EOpCode.OP_SETTABUP, a, b, c);
        }

        public void EmitCall(int a, int nArgs, int nRet)
        {
            EmitABC(EOpCode.OP_CALL, a, nArgs + 1, nRet + 1);
        }

        public void EmitTailCall(int a, int nArgs)
        {
            EmitABC(EOpCode.OP_TAILCALL, a, nArgs + 1, 0);
        }

        public void EmitReturn(int a, int n)
        {
            EmitABC(EOpCode.OP_RETURN, a, n + 1, 0);
        }

        public void EmitSelf(int a, int b, int c)
        {
            EmitABC(EOpCode.OP_SELF, a, b, c);
        }

        public int EmitJmp(int a, int sBx)
        {
            EmitAsBx(EOpCode.OP_JMP, a, sBx);
            return Insts.Count - 1;
        }

        public void EmitTest(int a, int c)
        {
            EmitABC(EOpCode.OP_TEST, a, 0, c);
        }

        public void EmitTestSet(int a, int b, int c)
        {
            EmitABC(EOpCode.OP_TESTSET, a, b, c);
        }

        public int EmitForPrep(int a, int sBx)
        {
            EmitAsBx(EOpCode.OP_FORPREP, a, sBx);
            return Insts.Count - 1;
        }

        public int EmitForLoop(int a, int sBx)
        {
            EmitAsBx(EOpCode.OP_FORLOOP, a, sBx);
            return Insts.Count - 1;
        }

        public void EmitTForCall(int a, int c)
        {
            EmitABC(EOpCode.OP_TFORCALL, a, 0, c);
        }

        public void EmitTForLoop(int a, int sBx)
        {
            EmitAsBx(EOpCode.OP_TFORLOOP, a, sBx);
        }

        public void EmitUnaryOp(EOpCode op, int a, int b)
        {
            switch (op)
            {
                case EOpCode.OP_NOT:
                    EmitABC(EOpCode.OP_NOT, a, b, 0);
                    break;

                case EOpCode.OP_BNOT:
                    EmitABC(EOpCode.OP_BNOT, a, b, 0);
                    break;

                case EOpCode.OP_LEN:
                    EmitABC(EOpCode.OP_LEN, a, b, 0);
                    break;

                case EOpCode.OP_UNM:
                    EmitABC(EOpCode.OP_UNM, a, b, 0);
                    break;
            }
        }

        public void EmitBinaryOp(ETokenType op, int a, int b, int c)
        {
            if (ArithAndBitwiseBinops.TryGetValue(op, out var opCode))
            {
                EmitABC(opCode, a, b, c);
            }
            else
            {
                switch (op)
                {
                    case ETokenType.OpEq:
                        EmitABC(EOpCode.OP_EQ, 1, b, c);
                        break;

                    case ETokenType.OpNe:
                        EmitABC(EOpCode.OP_EQ, 0, b, c);
                        break;

                    case ETokenType.OpLt:
                        EmitABC(EOpCode.OP_LT, 1, b, c);
                        break;

                    case ETokenType.OpGt:
                        EmitABC(EOpCode.OP_LT, 1, c, b);
                        break;

                    case ETokenType.OpLe:
                        EmitABC(EOpCode.OP_LE, 1, b, c);
                        break;

                    case ETokenType.OpGe:
                        EmitABC(EOpCode.OP_LE, 1, c, b);
                        break;
                }

                EmitJmp(0, 1);
                EmitLoadBool(a, 0, 1);
                EmitLoadBool(a, 1, 0);
            }
        }

        public void CloseOpenUpvals()
        {
            var a = GetJmpArgA();
            if (a > 0)
                EmitJmp(a, 0);
        }
    }
}