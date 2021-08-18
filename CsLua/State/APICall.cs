using System;
using CsLua.API;
using CsLua.Binchunk;

namespace CsLua.State
{
    using Compiler = Compiler.Compiler;

    internal partial class LuaState : ILuaState
    {
        public EStatus Load(byte[] chunk, string chunkName, string mode)
        {
            if (string.IsNullOrEmpty(mode))
                mode = "bt";

            ProtoType? proto = null;
            do
            {
                if (mode.Contains("b"))
                {
                    if (ProtoType.IsBinaryChunk(chunk))
                        proto = ProtoType.Undump(chunk);
                }

                if (proto != null)
                    break;

                if (mode.Contains("t"))
                {
                    if (ProtoType.IsBinaryChunk(chunk))
                        return EStatus.ErrSyntax;

                    var chars = new char[chunk.Length];
                    Array.Copy(chunk, chars, chars.Length);
                    proto = Compiler.Compile(new string(chars), chunkName);
                }
            } while (false);

            if (proto == null)
                return EStatus.ErrSyntax;

            var c = new LuaClosure(proto);
            Stack.Push(c);
            if (proto.Upvalues.Length > 0)
            {
                var env = Registry.Get(LuaConst.LUA_RIDX_GLOBALS);
                c.Upvals![0] = new Upvalue { Val = env };
            }

            return EStatus.Ok;
        }

        public void CallK(int nArgs, int nResults, LuaKContext ctx, LuaKFunction? k)
        {
            Check(k == null || !CallInfo.IsLua(), "cannot use continuations inside hooks");
            CheckNElems(nArgs + 1);
            Check(RunningStatus == EStatus.Ok, "cannot do calls on non-normal thread");
            _Do.CheckResults(this, nArgs, nResults);

            var func = Top - (nArgs + 1);
            if (k != null && NNy == 0) // need to prepare continuation?
            {
                // save continuation
                CallInfo.CsFunction.K = k;
                // save context
                CallInfo.CsFunction.Ctx = ctx;
                // do the calll
                InnerCall(func, nResults);
            }
            else // no continuation or no yieldable
            {
                CallNoYield(func, nResults);
            }

            LuaAPI.AdjustResults(this, nResults);
        }

        public void Call(int nArgs, int nResults)
        {
            CallK(nArgs, nResults, 0, null);
        }

        public EStatus PCallK(int nArgs, int nResults, int errFuncIdx, LuaKContext ctx, LuaKFunction? k)
        {
            Check(k == null || !CallInfo.IsLua(), "cannot use continuations inside hooks");
            CheckNElems(nArgs + 1);
            Check(RunningStatus == EStatus.Ok, "cannot do calls on non-normal thread");
            _Do.CheckResults(this, nArgs, nResults);

            Calls c;
            var status = EStatus.Ok;

            int func;

            if (errFuncIdx == 0)
            {
                func = 0;
            }
            else
            {
                var o = GetValueByAbsIdx(errFuncIdx)!;
                CheckStackIndex(errFuncIdx, o);
                func = SaveStack(errFuncIdx);
            }

            // function to be called
            c.Func = Top - (nArgs + 1);

            // no continuation or no yieldable?
            if (k == null || NNy > 0)
            {
                // do a 'conventional' protected call
                c.NResults = nResults;
                status = PCall(_Do.FCall, c, SaveStack(c.Func), func);
            }
            else // prepare continuation (call is already protected by 'resume')
            {
                var ci = CallInfo;
                ci.CsFunction.K = k; // save continuation
                ci.CsFunction.Ctx = ctx; // save context
                // save information for error recovery
                ci.Extra = SaveStack(c.Func);
                ci.CsFunction.OldErrFunc = ErrFunc;
                ErrFunc = func;

                // TODO hook

                // function can do error recovery
                ci.CallStatus |= CallInfoStatus.YPCALL;
                // do the call
                InnerCall(c.Func, nResults);
                ci.CallStatus &= ~CallInfoStatus.YPCALL;
                ErrFunc = ci.CsFunction.OldErrFunc;

                // if it is here, there were no errors
                status = EStatus.Ok;
            }

            this.AdjustResults(nResults);
            return status;
        }

        public EStatus PCall(int nArgs, int nResults, int errFuncIdx)
        {
            return PCallK(nArgs, nResults, errFuncIdx, 0, null);
        }
    }
}