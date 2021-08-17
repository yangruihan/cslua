using CsLua.API;

namespace CsLua.State
{
    /// <summary>
    /// 协程相关
    /// </summary>
    internal partial class LuaState : ILuaState
    {
        public EStatus YieldK(int nResults, LuaKContext ctx, LuaKFunction? k)
        {
            var ci = CallInfo;

            // TODO user state yield

            CheckNElems(nResults);

            if (NNy > 0)
            {
                if (GlobalState.MainThread != this)
                    RunError($"attempt to yield across a C-call boundary");
                else
                    RunError($"attempt to yield from outside a coroutine");
            }

            RunningStatus = EStatus.Yield;
            // save current 'func'
            ci.Extra = SaveStack(ci.Func);

            // inside a hook?
            if (ci.IsLua())
            {
                Check(k == null, "hooks cannot continue after yielding");
            }
            else
            {
                // is there a continuation?
                if ((ci.CsFunction.K = k) != null)
                    ci.CsFunction.Ctx = ctx; // save context

                ci.Func = Top - nResults - 1; // protect stack below results
                Throw(EStatus.Yield);
            }

            Assert((ci.CallStatus & CallInfoStatus.HOOKED) == CallInfoStatus.HOOKED);
            return EStatus.Ok;
        }

        public EStatus Resume(ILuaState from, int nArgs)
        {
            var fromState = from as LuaState;

            // save "number of non-yieldable" calls
            var oldNNy = NNy;

            // may be starting a coroutine
            if (RunningStatus == EStatus.Ok)
            {
                // not in base level?
                if (CallInfo != BaseCI)
                {
                    return _Do.ResumeError(this,
                                           "cannot resume non-suspended coroutine",
                                           nArgs);
                }
            }
            else if (RunningStatus != EStatus.Yield)
            {
                return _Do.ResumeError(this,
                                       "cannot resume dead coroutine",
                                       nArgs);
            }

            NCcalls = (ushort)(fromState != null
                ? fromState.NCcalls + 1
                : 1);
            if (NCcalls >= LuaConst.MAXCCALLS)
                return _Do.ResumeError(this,
                                       "C stack overflow", nArgs);

            // TODO user state resume

            NNy = 0; // allow yields

            CheckNElems(RunningStatus == EStatus.Ok ? nArgs + 1 : nArgs);

            var status = RawRunProtected(_Do.Resume, nArgs);

            // error calling 'lua_resume'? 
            if (status == EStatus.Undefine)
            {
                status = EStatus.ErrRun;
            }
            else // continue running after recoverable errors
            {
                while (_Do.IsErrorStatus(status) && _Do.Recover(this, status))
                {
                    // unroll continuation
                    status = RawRunProtected(_Do.UnRoll, status);
                }

                if (_Do.IsErrorStatus(status)) // unrecoverable error?
                {
                    RunningStatus = status; // mark thread as 'dead'
                    _Do.SetErrorObj(this, status, Top); // push error message
                    CallInfo.Top = Top;
                }
                else
                {
                    Assert(status == RunningStatus); // normal end or yield
                }
            }
            NNy = oldNNy;
            NCcalls--;
            Assert(NCcalls == (fromState != null ? fromState.NCcalls : 0));
            return status;
        }

        public EStatus Status()
        {
            return RunningStatus;
        }

        public bool IsYieldable()
        {
            return NNy == 0;
        }

        public EStatus Yield(int nResults)
        {
            return YieldK(nResults, 0, null);
        }
    }
}