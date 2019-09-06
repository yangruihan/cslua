using System.Collections.Generic;
using CsLua.API;

namespace CsLua.State
{
    partial class LuaState : ILuaVM
    {
        public int PC()
        {
            return _stack.PC;
        }

        public void AddPC(int n)
        {
            _stack.PC += n;
        }

        public uint Fetch()
        {
            var i = _stack.Closure.Proto.Code[_stack.PC];
            _stack.PC++;
            return i;
        }

        public void GetConst(int idx)
        {
            var c = _stack.Closure.Proto.Constatns[idx];
            _stack.Push(c);
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
            return (int) _stack.Closure.Proto.MaxStackSize;
        }

        public void LoadVararg(int n)
        {
            if (n < 0)
                n = _stack.Varargs.Length;

            _stack.Check(n);
            _stack.PushN(_stack.Varargs, n);
        }

        public void LoadProto(int idx)
        {
            var proto = _stack.Closure.Proto.Protos[idx];
            var closure = new Closure(proto);
            _stack.Push(closure);

            for (var i = 0; i < proto.Upvalues.Length; i++)
            {
                var uvInfo = proto.Upvalues[i];
                if (uvInfo.Instack == 1)
                {
                    if (_stack.Openuvs is null)
                        _stack.Openuvs = new Dictionary<int, Upvalue>();

                    if (_stack.Openuvs.ContainsKey(uvInfo.Idx))
                    {
                        closure.Upvals[i] = _stack.Openuvs[uvInfo.Idx];
                    }
                    else
                    {
                        closure.Upvals[i] = new Upvalue {Val = _stack.Slots[uvInfo.Idx]};
                        _stack.Openuvs[uvInfo.Idx] = closure.Upvals[i];
                    }
                }
                else
                {
                    closure.Upvals[i] = _stack.Closure.Upvals[uvInfo.Idx];
                }
            }
        }

        public void CloseUpvalues(int a)
        {
            for (var i = 0; i < _stack.Openuvs.Count; i++)
            {
                if (i >= a - 1)
                {
                    _stack.Openuvs.Remove(i);
                }
            }
        }
    }
}