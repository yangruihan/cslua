using System;
using CsLua.API;
using CsLua.Common;
using CsLua.Number;

namespace CsLua.State
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    delegate LuaInt IntegerFunc(LuaInt a, LuaInt b);

    delegate LuaFloat FloatFunc(LuaFloat a, LuaFloat b);

    struct Operator
    {
        public IntegerFunc IntegerFunc;
        public FloatFunc FloatFunc;

        public Operator(IntegerFunc integerFunc, FloatFunc floatFunc)
        {
            IntegerFunc = integerFunc;
            FloatFunc = floatFunc;
        }
    }

    static class Operators
    {
        private static readonly IntegerFunc IAdd = (a, b) => a + b;
        private static readonly FloatFunc FAdd = (a, b) => a + b;
        private static readonly IntegerFunc ISub = (a, b) => a - b;
        private static readonly FloatFunc FSub = (a, b) => a - b;
        private static readonly IntegerFunc IMul = (a, b) => a * b;
        private static readonly FloatFunc FMul = (a, b) => a * b;
        private static readonly IntegerFunc IMod = LuaMath.IMod;
        private static readonly FloatFunc FMod = LuaMath.FMod;
        private static readonly FloatFunc Pow = Math.Pow;
        private static readonly FloatFunc Div = (a, b) => a / b;
        private static readonly IntegerFunc IIDiv = LuaMath.IFloorDiv;
        private static readonly FloatFunc FIDiv = LuaMath.FFloorDiv;
        private static readonly IntegerFunc BAnd = (a, b) => a & b;
        private static readonly IntegerFunc BOr = (a, b) => a | b;
        private static readonly IntegerFunc BXor = (a, b) => a ^ b;
        private static readonly IntegerFunc Shl = LuaMath.ShiftLeft;
        private static readonly IntegerFunc Shr = LuaMath.ShiftRight;
        private static readonly IntegerFunc IUnm = (a, _) => -a;
        private static readonly FloatFunc FUnm = (a, _) => -a;
        private static readonly IntegerFunc BNot = (a, _) => ~a;

        public static Operator[] Ops =
        {
            new Operator(IAdd, FAdd),
            new Operator(ISub, FSub),
            new Operator(IMul, FMul),
            new Operator(IMod, FMod),
            new Operator(null, Pow),
            new Operator(null, Div),
            new Operator(IIDiv, FIDiv),
            new Operator(BAnd, null),
            new Operator(BOr, null),
            new Operator(BXor, null),
            new Operator(Shl, null),
            new Operator(Shr, null),
            new Operator(IUnm, FUnm),
            new Operator(BNot, null),
        };
    }

    partial class LuaState : ILuaState
    {
        public void Arith(EArithOp op)
        {
            var b = _stack.Pop();
            var a = b;

            if (op != EArithOp.Unm && op != EArithOp.BNot)
                a = _stack.Pop();

            var o = Operators.Ops[(int) op];
            var ret = InnerArith(a, b, o);
            if (ret != null)
                _stack.Push(ret);
            else
                Debug.Panic("arithmetic error!");
        }

        private LuaValue InnerArith(LuaValue a, LuaValue b, Operator op)
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