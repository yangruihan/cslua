using System;
using CsLua.API;
using CsLua.Common;
using CsLua.Number;

namespace CsLua.State
{
    internal delegate LuaInt IntegerFunc(LuaInt a, LuaInt b);

    internal delegate LuaFloat FloatFunc(LuaFloat a, LuaFloat b);

    internal struct Operator
    {
        public string MetaMethod;
        public IntegerFunc IntegerFunc;
        public FloatFunc FloatFunc;

        public Operator(string metaMethod, IntegerFunc integerFunc, FloatFunc floatFunc)
        {
            MetaMethod = metaMethod;
            IntegerFunc = integerFunc;
            FloatFunc = floatFunc;
        }
    }

    internal static class Operators
    {
        private static readonly IntegerFunc IAdd = (a, b) => a + b;
        private static readonly FloatFunc FAdd = (a, b) => a + b;
        private static readonly IntegerFunc ISub = (a, b) => a - b;
        private static readonly FloatFunc FSub = (a, b) => a - b;
        private static readonly IntegerFunc IMul = (a, b) => a * b;
        private static readonly FloatFunc FMul = (a, b) => a * b;
        private static readonly IntegerFunc IMod = LuaMath.IMod;
        private static readonly FloatFunc FMod = LuaMath.FMod;
        private static readonly FloatFunc Pow = LuaMath.Pow;
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
            new Operator("__add", IAdd, FAdd),
            new Operator("__sub", ISub, FSub),
            new Operator("__mul", IMul, FMul),
            new Operator("__mod", IMod, FMod),
            new Operator("__pow", null, Pow),
            new Operator("__div", null, Div),
            new Operator("__idiv", IIDiv, FIDiv),
            new Operator("__band", BAnd, null),
            new Operator("__bor", BOr, null),
            new Operator("__bxor", BXor, null),
            new Operator("__shl", Shl, null),
            new Operator("__shr", Shr, null),
            new Operator("__unm", IUnm, FUnm),
            new Operator("__bnot", BNot, null),
        };
    }

    /// <summary>
    /// 算数运算实现
    /// </summary>
    partial class LuaState : ILuaState
    {
        /// <summary>
        /// 执行算术和按位运算
        /// </summary>
        public void Arith(EArithOp op)
        {
            var b = Stack.Pop();
            var a = b;

            if (op != EArithOp.Unm && op != EArithOp.BNot)
                a = Stack.Pop();

            var o = Operators.Ops[(int) op];
            var ret = InnerArith(a, b, o);
            if (ret != null)
            {
                Stack.Push(ret);
                return;
            }

            var mm = Operators.Ops[(int) op].MetaMethod;
            if (LuaValue.CallMetaMethod(a, b, mm, this, out ret))
            {
                Stack.Push(ret);
                return;
            }

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