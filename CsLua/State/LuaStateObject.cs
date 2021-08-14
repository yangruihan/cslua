using CsLua.API;

namespace CsLua.State
{
    internal partial class LuaState
    {
        private void SetValue(int idx, LuaValue v)
        {
            Index2Addr(idx, out var absIdx);
            Stack[absIdx] = v;
        }

        private LuaValue? InnerArith(LuaValue a, LuaValue b, Operator op)
        {
            if (op.FloatFunc is null)
            {
                if (a.ToInteger(out var aI))
                    if (b.ToInteger(out var bI))
                        return new LuaValue(op.IntegerFunc(aI, bI));
            }
            else
            {
                if (!(op.IntegerFunc is null))
                {
                    if (a.ToInteger(out var aI))
                        if (b.ToInteger(out var bI))
                            return new LuaValue(op.IntegerFunc(aI, bI));
                }

                if (a.ToFloat(out var aF))
                    if (b.ToFloat(out var bF))
                        return new LuaValue(op.FloatFunc(aF, bF));
            }

            return null;
        }
    }
}