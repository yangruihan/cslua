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
            var proto = ProtoType.Undump(chunk);
            var c = new Closure(proto);
            _stack.Push(c);
            return 0;
        }

        public void Call(int nArgs, int nResults)
        {
            var val = _stack[-(nArgs + 1)];
            if (val.Value is Closure c)
            {
                Console.Write($"call {c.Proto.Source}<{c.Proto.LineDefined},{c.Proto.LastLineDefined}>\n");
                CallLuaClosure(nArgs, nResults, c);
            }
            else
            {
                Debug.Panic("not function!");
            }
        }

        private void CallLuaClosure(int nArgs, int nResults, Closure c)
        {
            var nRegs = (int) c.Proto.MaxStackSize;
            var nParams = (int) c.Proto.NumParams;
            var isVararg = c.Proto.IsVararg == 1;

            var newStack = new LuaStack(nRegs + 20) {Closure = c};

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