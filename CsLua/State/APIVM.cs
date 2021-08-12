using System.Collections.Generic;
using CsLua.API;

namespace CsLua.State
{
    internal partial class LuaState : ILuaVM
    {
        public int PC()
        {
            return Stack.PC;
        }

        public void AddPC(int n)
        {
            Stack.PC += n;
        }

        public uint Fetch()
        {
            var i = Stack.Closure.Proto.Code[Stack.PC];
            Stack.PC++;
            return i;
        }

        public void GetConst(int idx)
        {
            var c = Stack.Closure.Proto.Constatns[idx];
            Stack.Push(c);
        }

        public void GetRK(int rk)
        {
            if (rk > 0xff)
                GetConst(rk & 0xff);
            else
                PushValue(rk + 1);
        }

        public int RegisterCount()
        {
            return (int) Stack.Closure.Proto.MaxStackSize;
        }

        public void LoadVararg(int n)
        {
            if (n < 0)
                n = Stack.Varargs.Length;

            Stack.Check(n);
            Stack.PushN(Stack.Varargs, n);
        }

        public void LoadProto(int idx)
        {
            var proto = Stack.Closure.Proto.Protos[idx];
            var closure = new Closure(proto);
            Stack.Push(closure);

            for (var i = 0; i < proto.Upvalues.Length; i++)
            {
                var uvInfo = proto.Upvalues[i];
                if (uvInfo.Instack == 1)
                {
                    if (Stack.Openuvs is null)
                        Stack.Openuvs = new Dictionary<int, Upvalue>();

                    if (Stack.Openuvs.ContainsKey(uvInfo.Idx))
                    {
                        closure.Upvals[i] = Stack.Openuvs[uvInfo.Idx];
                    }
                    else
                    {
                        closure.Upvals[i] = new Upvalue {Val = Stack.Slots[uvInfo.Idx]};
                        Stack.Openuvs[uvInfo.Idx] = closure.Upvals[i];
                    }
                }
                else
                {
                    closure.Upvals[i] = Stack.Closure.Upvals[uvInfo.Idx];
                }
            }
        }

        public void CloseUpvalues(int a)
        {
            for (var i = 0; i < Stack.Openuvs.Count; i++)
            {
                if (i >= a - 1)
                {
                    Stack.Openuvs.Remove(i);
                }
            }
        }
    }
}