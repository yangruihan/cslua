using System;
using CsLua.API;
using CsLua.Misc;
using CsLua.VM;

namespace CsLua.State
{
    internal partial class LuaState
    {
        internal struct Calls
        {
            public int Func;
            public int NResults;
        }

        public delegate void PFunc(LuaState l, object ud);

        private static void FCall(LuaState l, object ud)
        {
            var c = (Calls) ud;
            l.CallNoYield(c.Func, c.NResults);
        }

        private void StackError()
        {
            // todo
            Debug.Panic("C stack overflow");
        }

        private EStatus RawRunProtected(PFunc f, object userData)
        {
            var oldNCcalls = NCcalls;
            try
            {
                f(this, userData);
            }
            catch (LuaException e)
            {
                return e.Status;
            }

            NCcalls = oldNCcalls;
            return EStatus.Ok;
        }

        private void SetErrorObj(EStatus errCode, int oldTop)
        {
            switch (errCode)
            {
                case EStatus.ErrMem:
                    SetValue(oldTop, LuaValue.CreateStr(GlobalState.MemErrMsg));
                    break;

                case EStatus.ErrErr:
                    SetValue(oldTop, LuaValue.CreateStr("error in error handling"));
                    break;

                default:
                    SetValue(oldTop, Stack[Top - 1]);
                    break;
            }

            Index2Addr(oldTop, out var oldTopIdx);
            SetTop(oldTopIdx);
        }

        private EStatus PCall(PFunc func, object userData, int oldTop, int errorFunc)
        {
            var oldCi = CallInfo;
            var oldNny = NNy;
            var oldErrFunc = ErrFunc;
            ErrFunc = errorFunc;
            var status = RawRunProtected(func, userData);
            if (status != EStatus.Ok)
            {
                var oldTopIdx = RestoreStack(oldTop);
                Close(oldTopIdx);
                SetErrorObj(status, oldTop);
                CallInfo = oldCi;
                NNy = oldNny;
            }

            ErrFunc = oldErrFunc;
            return status;
        }

        private int SaveStack(int idx)
        {
            return CallInfo.Func + idx;
        }

        private int RestoreStack(int idx)
        {
            return idx - CallInfo.Func;
        }

        private void CheckResults(int na, int nr)
        {
            LuaAPI.Check(this, nr == LuaConst.LUA_MULTRET || CallInfo.Top - Top >= nr - na,
                "results from function overflow current stack size");
        }

        private void CallNoYield(int funcIdx, int nResults)
        {
            NNy++;
            InnerCall(funcIdx, nResults);
            NNy--;
        }

        private bool PreCall(int nArgs, int nResults)
        {
            return true;
        }

        private void InnerCall(int funcIdx, int nResults)
        {
            if (++NCcalls >= LuaConst.MAXCCALLS)
            {
                StackError();
            }

            var val = Stack[funcIdx];
            var ok = val.IsFunction();
            var c = val.GetClosureValue();

            if (!ok)
            {
                var mf = LuaValue.GetMetaField(val, "__call", this);
                if (mf != null)
                {
                    ok = mf.IsFunction();
                    c = mf.GetClosureValue();

                    if (ok)
                    {
                        Stack.Push(val);
                        Insert(--funcIdx);
                    }
                }
            }

            if (ok)
            {
                if (c.Proto != null)
                    CallLuaClosure(funcIdx, nResults, c);
                else
                    CallCSClosure(funcIdx, nResults, c);
            }
            else
            {
                Debug.Panic("not function!");
            }
        }

        private void CallLuaClosure(int funcIdx, int nResults, Closure c)
        {
            var nArgs = Top - funcIdx - 1;
            var nRegs = (int) c.Proto.MaxStackSize;
            var nParams = (int) c.Proto.NumParams;
            var isVararg = c.Proto.IsVararg == 1;

            var newStack = new LuaStack(nRegs + LuaConst.LUA_MINSTACK, this)
                {Closure = c};

            // pass args, pop func
            var funcAndArgs = Stack.PopN(nArgs + 1);
            newStack.PushN(funcAndArgs.Slice(1), nParams);
            newStack.Top = nRegs;
            if (nArgs > nParams && isVararg)
                newStack.Varargs = funcAndArgs.Slice(nParams + 1);

            // run closure
            PushLuaStack(newStack);
            RunLuaClosure();
            PopLuaStack();

            // return results
            if (nResults != 0)
            {
                var results = newStack.PopN(newStack.Top - nRegs);
                Stack.Check(results.Length);
                Stack.PushN(results, nResults);
            }
        }

        private void CallCSClosure(int funcIdx, int nResults, Closure c)
        {
            var nArgs = Top - funcIdx - 1;
            var newStack = new LuaStack(nArgs + LuaConst.LUA_MINSTACK, this)
                {Closure = c};

            var args = Stack.PopN(nArgs);
            newStack.PushN(args, nArgs);
            Stack.Pop();

            PushLuaStack(newStack);
            var r = c.LuaCsFunction(this);
            PopLuaStack();

            if (nResults != 0)
            {
                var results = newStack.PopN(r);
                Stack.Check(results.Length);
                Stack.PushN(results, nResults);
            }
        }

        private void RunLuaClosure()
        {
            for (;;)
            {
                var inst = new Instruction(Fetch());
                inst.Execute(this);
                if (inst.Opcode() == EOpCode.OP_RETURN)
                    break;
            }
        }
    }
}