using System;
using System.Collections.Generic;
using CsLua.API;
using CsLua.Binchunk;
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

        internal delegate void PFunc(LuaState l, object ud);

        private static class _Do
        {
            public static void StackError(LuaState l)
            {
                if (l.NCcalls == LuaConst.MAXCCALLS)
                    l.RunError("C stack overflow");
                else if (l.NCcalls >= (LuaConst.MAXCCALLS + (LuaConst.MAXCCALLS >> 3)))
                    l.Throw(EStatus.ErrErr, "error while handing stack error"); //  error while handing stack error
            }

            public static void FCall(LuaState l, object ud)
            {
                var c = (Calls)ud;
                l.CallNoYield(c.Func, c.NResults);
            }

            public static void SetErrorObj(LuaState l, EStatus errCode, int oldTop)
            {
                switch (errCode)
                {
                    case EStatus.ErrMem:
                        l.SetValue(oldTop, LuaValue.Create(l.GlobalState.MemErrMsg));
                        break;

                    case EStatus.ErrErr:
                        l.SetValue(oldTop, LuaValue.Create("error in error handling"));
                        break;

                    default:
                        l.SetValue(oldTop, l.Stack[l.Top - 1]);
                        break;
                }

                l.SetTop(oldTop);
            }

            public static void CheckResults(LuaState l, int na, int nr)
            {
                l.Check(nr == LuaConst.LUA_MULTRET || l.CallInfo.Top - l.Top >= nr - na,
                    "results from function overflow current stack size");
            }

            public static void TryFuncTM(LuaState l, int func)
            {
                var tm = l.GetTMByObj(l.Index2Addr(func)!, ETagMethods.CALL);
                if (tm == null || !tm.IsFunction())
                    l.TypeError(func, "call");

                // Open a hole inside the stack at 'func'
                for (int p = l.Top; p > func; p--)
                    l.SetValue(p, p - 1);
                l.Top++; // slot ensured by caller
                l.SetValue(func, tm); // tag method is the new function to be called
            }

            public static bool MoveResults(LuaState l, int firstResult, int res, int nRes, int wanted)
            {
                // handle typical cases separately
                switch (wanted)
                {
                    case 0: break; // nothing to move
                    case 1:
                        {
                            if (nRes == 0)
                                l.SetValue(firstResult, null);

                            // move it to proper place
                            l.SetValue(res, firstResult);
                            break;
                        }

                    case LuaConst.LUA_MULTRET:
                        {
                            // move all results to correct place
                            for (int i = 0; i < nRes; i++)
                            {
                                l.SetValue(res + i, firstResult + i);
                            }
                            l.SetTop(res + nRes);
                            return false;
                        }

                    default:
                        {
                            if (wanted <= nRes) // enough results?
                            {
                                //  move wanted results to correct place
                                for (int i = 0; i < wanted; i++)
                                    l.SetValue(res + i, firstResult + i);
                            }
                            else // not enough results; use all of them plus nils
                            {
                                int i;
                                // move all results to correct place
                                for (i = 0; i < nRes; i++)
                                    l.SetValue(res + i, firstResult + i);

                                // complete wanted number of results
                                for (; i < wanted; i++)
                                    l.SetValue(res + i, LuaValue.Nil);
                            }
                            break;
                        }
                }

                l.SetTop(res + wanted); // top points after the last result
                return true;
            }

            public static CallInfo NextCI(LuaState l)
            {
                return l.CallInfo = l.CallInfo.Next ?? l.ExtendCI();
            }

            public static int AdjustVarargs(LuaState l, ProtoType p, int actual)
            {
                int nFixArgs = p.NumParams;
                int @base, @fixed;

                // move fixed parameters to final position
                @fixed = l.Top - actual; // first fixed argument
                @base = l.Top; // final position of first argument

                int i;
                for (i = 0; i < nFixArgs && i < actual; i++)
                {
                    l.Push(l.Index2Addr(@fixed + i)!);
                    l.SetValue(@fixed + i, LuaValue.Nil);
                }

                // complete missing arguments
                for (; i < nFixArgs; i++)
                    l.PushNil();

                return @base;
            }

        }

        private int SaveStack(int idx)
        {
            return CallInfo.Func + idx;
        }

        private int RestoreStack(int idx)
        {
            return idx - CallInfo.Func;
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
                _Do.SetErrorObj(this, status, oldTop);
                CallInfo = oldCi;
                NNy = oldNny;
                // TODO Shrinkstack
            }

            ErrFunc = oldErrFunc;
            return status;
        }

        private void CallNoYield(int funcIdx, int nResults)
        {
            NNy++;
            InnerCall(funcIdx, nResults);
            NNy--;
        }

        private bool PosCall(CallInfo ci, int firstResult, int nRes)
        {
            int res = ci.Func; // res == final position of 1st result
            int wanted = ci.NResults;
            CallInfo = ci.Previous!; // back to caller
            // move results to proper place
            return _Do.MoveResults(this, firstResult, res, nRes, wanted);
        }

        private bool PreCall(int func, int nResults)
        {
            LuaCSFunction? f = null;
            CallInfo? ci = null;

            var funcVal = Index2Addr(func)!;

            switch (funcVal.Type)
            {
                case ELuaType.CSClosure: // CS Closure
                case ELuaType.LCSFunction: // light cs function
                    {
                        f = funcVal.Type == ELuaType.CSClosure
                            ? funcVal.GetCSClosureFunctionValue()!
                            : funcVal.GetLCSFunctionValue()!;

                        // number of returns
                        int n;
                        // ensure minimum stack size
                        CheckStack(LuaConst.LUA_MINSTACK);

                        ci = _Do.NextCI(this);
                        ci.NResults = nResults;
                        ci.Func = func;
                        ci.Top = Top + LuaConst.LUA_MINSTACK;
                        ci.CallStatus = CallInfoStatus.INIT;

                        // TODO hook

                        // do the actual call
                        n = f(this);

                        CheckNElems(n);

                        PosCall(ci, Top - n, n);
                        return true;
                    }

                case ELuaType.LuaClosure: // lua function
                    {
                        int @base;

                        var p = Index2Addr(func)!.GetLuaClosureValue()!.Proto;
                        int n = Top - func - 1; // number of real arguments
                        int fsize = p.MaxStackSize; // frame size
                        CheckStack(fsize);
                        if (p.IsVararg == 1)
                        {
                            @base = _Do.AdjustVarargs(this, p, n);
                            var varargs = new List<LuaValue>();
                            int cnt = n - p.NumParams;
                            if (cnt > 0)
                            {
                                Stack.PopN(cnt, out var vars);
                                CallInfo.LuaClosure.Varargs = vars!;
                            }
                        }
                        else // non vararg function
                        {
                            for (; n < p.NumParams; n++)
                                PushNil();
                            @base = func + 1;
                        }
                        ci = _Do.NextCI(this);
                        ci.NResults = nResults;
                        ci.Func = func;
                        Top = ci.Top = @base + fsize;
                        ci.LuaClosure.Closure = Index2Addr(func)!.GetLuaClosureValue()!;
                        ci.LuaClosure.SavedPc = 0;
                        ci.CallStatus = CallInfoStatus.LUA;
                        // TODO hook
                        return false;
                    }
                default: // not a function
                    {
                        // ensure space for metamethod
                        CheckStack(1);
                        // try to get '__call' metamethod
                        _Do.TryFuncTM(this, func);

                        // now it must be a function
                        return PreCall(func, nResults);
                    }
            }
        }

        private void InnerCall(int funcIdx, int nResults)
        {
            if (++NCcalls >= LuaConst.MAXCCALLS)
                _Do.StackError(this);

            // is a Lua function?
            if (!PreCall(funcIdx, nResults))
                Execute(); // call it

            NCcalls--;
        }

        private void Throw(EStatus errorCode, string msg = "")
        {
            throw new LuaException(msg, errorCode);
        }
    }
}