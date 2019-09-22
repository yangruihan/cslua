using System;
using CsLua.API;
using CsLua.Binchunk;
using CsLua.Common;
using CsLua.VM;

namespace CsLua.State
{
    partial class LuaState : ILuaState
    {
        public int Load(byte[] chunk, string chunkName, string mode)
        {
            ProtoType proto = null;
            if (ProtoType.IsBinaryChunk(chunk))
            {
                proto = ProtoType.Undump(chunk);
            }
            else
            {
                var chars = new char[chunk.Length];
                Array.Copy(chunk, chars, chars.Length);
                proto = Compiler.Compiler.Compile(new string(chars), chunkName);
            }

            var c = new Closure(proto);
            _stack.Push(c);
            if (proto.Upvalues.Length > 0)
            {
                var env = _registry.Get(Consts.LUA_RIDX_GLOBALS);
                c.Upvals[0] = new Upvalue {Val = env};
            }

            return (int) EErrorCode.Ok;
        }

        public void Call(int nArgs, int nResults)
        {
            var val = _stack[-(nArgs + 1)];
            var ok = val.Value is Closure;
            var c = val.Value as Closure;

            if (!ok)
            {
                var mf = LuaValue.GetMetaField(val, "__call", this);
                if (mf != null)
                {
                    ok = mf.Value is Closure;
                    c = mf.Value as Closure;

                    if (ok)
                    {
                        _stack.Push(val);
                        Insert(-(nArgs + 2));
                        nArgs += 1;
                    }
                }
            }

            if (ok)
            {
                if (c.Proto != null)
                    CallLuaClosure(nArgs, nResults, c);
                else
                    CallCSClosure(nArgs, nResults, c);
            }
            else
            {
                Debug.Panic("not function!");
            }
        }

        public int PCall(int nArgs, int nResults, int msgh)
        {
            var caller = _stack;
            var status = EErrorCode.ErrRun;

            try
            {
                Call(nArgs, nResults);
                status = EErrorCode.Ok;
            }
            catch (Exception e)
            {
                while (_stack != caller)
                    PopLuaStack();

                _stack.Push(e.Message);
            }

            return (int) status;
        }

        private void CallLuaClosure(int nArgs, int nResults, Closure c)
        {
            var nRegs = (int) c.Proto.MaxStackSize;
            var nParams = (int) c.Proto.NumParams;
            var isVararg = c.Proto.IsVararg == 1;

            var newStack = new LuaStack(nRegs + Consts.LUA_MINSTACK, this) {Closure = c};

            // pass args, pop func
            var funcAndArgs = _stack.PopN(nArgs + 1);
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
                _stack.Check(results.Length);
                _stack.PushN(results, nResults);
            }
        }

        private void CallCSClosure(int nArgs, int nResults, Closure c)
        {
            var newStack = new LuaStack(nArgs + Consts.LUA_MINSTACK, this) {Closure = c};

            var args = _stack.PopN(nArgs);
            newStack.PushN(args, nArgs);
            _stack.Pop();

            PushLuaStack(newStack);
            var r = c.CSFunction(this);
            PopLuaStack();

            if (nResults != 0)
            {
                var results = newStack.PopN(r);
                _stack.Check(results.Length);
                _stack.PushN(results, nResults);
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