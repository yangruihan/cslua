using System;
using CsLua.API;
using CsLua.Binchunk;
using CsLua.Misc;
using CsLua.VM;

namespace CsLua.State
{
    using Compiler = Compiler.Compiler;

    internal partial class LuaState : ILuaState
    {
        public EErrorCode Load(byte[] chunk, string chunkName, string mode)
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
                        return EErrorCode.ErrSyntax;

                    var chars = new char[chunk.Length];
                    Array.Copy(chunk, chars, chars.Length);
                    proto = Compiler.Compile(new string(chars), chunkName);
                }
            } while (false);

            if (proto == null)
                return EErrorCode.ErrSyntax;

            var c = new Closure(proto);
            Stack.Push(c);
            if (proto.Upvalues.Length > 0)
            {
                var env = Registry.Get(LuaConst.LUA_RIDX_GLOBALS);
                c.Upvals[0] = new Upvalue {Val = env};
            }

            return EErrorCode.Ok;
        }

        public void CallK(int nArgs, int nResults, LuaContext ctx, LuaKFunction? k)
        {
            var val = Stack[-(nArgs + 1)];
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

        public void Call(int nArgs, int nResults)
        {
            CallK(nArgs, nResults, 0, null);
        }

        public EErrorCode PCall(int nArgs, int nResults, int msgh)
        {
            var caller = Stack;
            var status = EErrorCode.ErrRun;

            try
            {
                Call(nArgs, nResults);
                status = EErrorCode.Ok;
            }
            catch (Exception e)
            {
                while (Stack != caller)
                    PopLuaStack();

                Stack.Push(e.Message);
            }

            return status;
        }

        private void CallLuaClosure(int nArgs, int nResults, Closure c)
        {
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

        private void CallCSClosure(int nArgs, int nResults, Closure c)
        {
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