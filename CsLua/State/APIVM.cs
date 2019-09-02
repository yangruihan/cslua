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
        }
    }
}