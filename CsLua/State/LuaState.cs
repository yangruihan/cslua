using CsLua.API;
using CsLua.Binchunk;

namespace CsLua.State
{
    partial class LuaState
    {
        private LuaStack _stack;
        private ProtoType _proto;
        private int _pc;

        public LuaState(int stackSize, ProtoType proto)
        {
            _stack = new LuaStack(stackSize);
            _proto = proto;
            _pc = 0;
        }
    }
}