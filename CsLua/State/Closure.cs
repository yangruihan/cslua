using CsLua.API;
using CsLua.Binchunk;

namespace CsLua.State
{
    internal struct Upvalue
    {
        public LuaValue Val;
    }

    internal class Closure
    {
        public ProtoType Proto;
        public LuaCSFunction LuaCsFunction;
        public Upvalue[] Upvals;

        public Closure(ProtoType proto)
        {
            Proto = proto;

            if (proto.Upvalues.Length > 0)
                Upvals = new Upvalue[proto.Upvalues.Length];
        }

        public Closure(LuaCSFunction luaCsFunction, int nUpvals)
        {
            LuaCsFunction = luaCsFunction;

            if (nUpvals > 0)
                Upvals = new Upvalue[nUpvals];
        }
    }
}