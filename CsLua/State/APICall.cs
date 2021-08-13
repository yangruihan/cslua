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

            ProtoType proto = null;
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

            var c = new Closure(proto);
            Stack.Push(c);
            if (proto.Upvalues.Length > 0)
            {
                var env = Registry.Get(LuaConst.LUA_RIDX_GLOBALS);
                c.Upvals[0] = new Upvalue {Val = env};
            }

            return EStatus.Ok;
        }


        public void CallK(int nArgs, int nResults, LuaKContext ctx, LuaKFunction? k)
        {
            LuaAPI.Check(this, k == null || !CallInfo.IsLua(), "cannot use continuations inside hooks");
            LuaAPI.CheckNElems(this, nArgs + 1);
            LuaAPI.Check(this, Status == EStatus.Ok, "cannot do calls on non-normal thread");
            CheckResults(nArgs, nResults);

            var funcIdx = Top - (nArgs + 1);
            if (k != null && NNy == 0) // need to prepare continuation?
            {
                CallInfo.CsFunction.K = k;
                CallInfo.CsFunction.Ctx = ctx;
            }
            else // no continuation or no yieldable
            {
                CallNoYield(funcIdx, nResults);
            }

            LuaAPI.AdjustResults(this, nResults);
        }

        public void Call(int nArgs, int nResults)
        {
            CallK(nArgs, nResults, 0, null);
        }

        public EStatus PCallK(int nArgs, int nResults, int errFuncIdx, LuaKContext ctx, LuaKFunction? k)
        {
            LuaAPI.Check(this, k == null || !CallInfo.IsLua(), "cannot use continuations inside hooks");
            LuaAPI.CheckNElems(this, nArgs + 1);
            LuaAPI.Check(this, this.Status == EStatus.Ok, "cannot do calls on non-normal thread");
            CheckResults(nArgs, nResults);

            Calls c;

            Int64 func;

            if (errFuncIdx == 0)
            {
                func = 0;
            }
            else
            {
                var o = Index2Addr(errFuncIdx);
                CheckStackIndex(errFuncIdx, o);
                func = SaveStack(errFuncIdx);
            }

            c.Func = Top - (nArgs + 1);
            if (k == null || NNy > 0)
            {
                c.NResults = nResults;
                Status = PCall(FCall, c, SaveStack(c.Func), func);
            }
            else // prepare continuation (call is already protected by 'resume')
            {
            }

            var caller = Stack;
            var status = EStatus.ErrRun;

            try
            {
                Call(nArgs, nResults);
                status = EStatus.Ok;
            }
            catch (Exception e)
            {
                while (Stack != caller)
                    PopLuaStack();

                Stack.Push(e.Message);
            }

            return status;
        }

        public EStatus PCall(int nArgs, int nResults, int errFuncIdx)
        {
            return PCallK(nArgs, nResults, errFuncIdx, 0, null);
        }
    }
}