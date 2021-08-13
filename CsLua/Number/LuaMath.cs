using System;
using CsLua.API;

namespace CsLua.Number
{
    /// <summary>
    /// 算数辅助类
    /// </summary>
    internal static class LuaMath
    {
        public static bool FloatToInteger(LuaFloat f, out LuaInt ret)
        {
            ret = f;
            return ret == f;
        }

        public static LuaInt IMod(LuaInt a, LuaInt b)
        {
            return a - IFloorDiv(a, b) * b;
        }

        public static LuaFloat FMod(LuaFloat a, LuaFloat b)
        {
            if (a > 0 && LuaFloat.IsPositiveInfinity(b) || a < 0 && LuaFloat.IsNegativeInfinity(b))
                return a;

            if (a > 0 && LuaFloat.IsNegativeInfinity(b) || a < 0 && LuaFloat.IsPositiveInfinity(b))
                return b;

            return a - Math.Floor(a / b) * b;
        }

        public static LuaFloat Pow(LuaFloat a, LuaFloat b)
        {
            return Math.Pow(a, b);
        }

        public static LuaInt IFloorDiv(LuaInt a, LuaInt b)
        {
            if (a > 0 && b > 0 || a < 0 && b < 0 || a % b == 0)
                return a / b;

            return a / b - 1;
        }

        public static LuaFloat FFloorDiv(LuaFloat a, LuaFloat b)
        {
            return Math.Floor(a / b);
        }

        public static LuaInt ShiftLeft(LuaInt a, LuaInt n)
        {
            if (n >= 0)
                return a << (int) n;
            return ShiftRight(a, -n);
        }

        public static LuaInt ShiftRight(LuaInt a, LuaInt n)
        {
            if (n >= 0)
                return a >> (int) n;
            return ShiftLeft(a, -n);
        }
    }
}