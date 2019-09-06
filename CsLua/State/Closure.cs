using CsLua.API;
using CsLua.Binchunk;

namespace CsLua.State
{
    class Closure
    {
        public ProtoType Proto;
        public CSFunction CSFunction;

        public Closure(ProtoType proto)
        {
            Proto = proto;
        }

        public Closure(CSFunction csFunction)
        {
            CSFunction = csFunction;
        }
    }
}
