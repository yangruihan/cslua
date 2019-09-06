using CsLua.API;
using CsLua.Binchunk;

namespace CsLua.State
{
    struct Upvalue
    {
        public LuaValue Val;
    }
    
    class Closure
    {
        public ProtoType Proto;
        public CSFunction CSFunction;
        public Upvalue[] Upvals;

        public Closure(ProtoType proto)
        {
            Proto = proto;
            
            if (proto.Upvalues.Length > 0)
                Upvals = new Upvalue[proto.Upvalues.Length];
        }

        public Closure(CSFunction csFunction, int nUpvals)
        {
            CSFunction = csFunction;

            if (nUpvals > 0)
                Upvals = new Upvalue[nUpvals];
        }
    }
}
