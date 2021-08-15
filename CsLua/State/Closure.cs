using CsLua.API;
using CsLua.Binchunk;

namespace CsLua.State
{
    internal struct Upvalue
    {
        public LuaValue? Val;
    }

    internal class Closure
    {
        public Upvalue[]? Upvals;
    }

    internal class LuaClosure : Closure
    {
        public ProtoType Proto;

        public LuaClosure(ProtoType proto)
        {
            Proto = proto;

            if (proto.Upvalues.Length > 0)
                Upvals = new Upvalue[proto.Upvalues.Length];
        }
    }

    internal class CSClosure : Closure
    {
        public LuaCSFunction LuaCsFunction;

        public CSClosure(LuaCSFunction luaCsFunction, int nUpvals)
        {
            LuaCsFunction = luaCsFunction;

            if (nUpvals > 0)
                Upvals = new Upvalue[nUpvals];
        }
    }
}