using CsLua.API;

namespace CsLua.State
{
    partial class LuaState : ILuaVM
    {
        public int PC()
        {
            return _pc;
        }

        public void AddPC(int n)
        {
            _pc += n;
        }

        public uint Fetch()
        {
            return _proto.Code[_pc++];
        }

        public void GetConst(int idx)
        {
            var c = _proto.Constatns[idx];
            _stack.Push(c);
        }

        public void GetRK(int rk)
        {
            if (rk > 0xff)
                GetConst(rk & 0xff);
            else
                PushValue(rk + 1);
        }
    }
}