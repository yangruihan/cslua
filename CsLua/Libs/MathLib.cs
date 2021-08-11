using System;
using CsLua.API;

namespace CsLua.Libs
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    public class MathLib
    {
        private static readonly CSFunction Abs = ls =>
        {
            if (ls.IsInteger(1))
            {
                var n = ls.ToInteger(1);
                n = Math.Abs(n);
                ls.PushInteger(n);
            }
            else
            {
                ls.PushNumber(Math.Abs(ls.CheckNumber(1)));
            }

            return 1;
        };

        private static readonly CSFunction Sin = ls =>
        {
            ls.PushNumber(Math.Sin(ls.CheckNumber(1)));
            return 1;
        };

        private static readonly CSFunction Cos = ls =>
        {
            ls.PushNumber(Math.Cos(ls.CheckNumber(1)));
            return 1;
        };

        private static readonly CSFunction Tan = ls =>
        {
            ls.PushNumber(Math.Tan(ls.CheckNumber(1)));
            return 1;
        };

        private static readonly CSFunction Asin = ls =>
        {
            ls.PushNumber(Math.Asin(ls.CheckNumber(1)));
            return 1;
        };

        private static readonly CSFunction Acos = ls =>
        {
            ls.PushNumber(Math.Acos(ls.CheckNumber(1)));
            return 1;
        };

        private static readonly CSFunction Atan = ls =>
        {
            LuaFloat y = ls.CheckNumber(1);
            LuaFloat x = ls.OptNumber(2, 1);
            ls.PushNumber(Math.Atan2(y, x));
            return 1;
        };

        private static readonly CSFunction ToInt = ls =>
        {
            if (ls.ToIntegerX(1, out var ret))
            {
                ls.PushInteger(ret);
            }
            else
            {
                ls.CheckAny(1);
                ls.PushNil();
            }

            return 1;
        };

        private static readonly LuaReg[] mathLib = new[]
        {
            new LuaReg()
            {
                Name = "abs",
                Func = Abs
            },
            new LuaReg()
            {
                Name = "sin",
                Func = Sin
            },
            new LuaReg()
            {
                Name = "cos",
                Func = Cos
            },
            new LuaReg()
            {
                Name = "tan",
                Func = Tan
            },
            new LuaReg()
            {
                Name = "asin",
                Func = Asin
            },
            new LuaReg()
            {
                Name = "acos",
                Func = Acos
            },
            new LuaReg()
            {
                Name = "atan",
                Func = Atan
            },
            new LuaReg()
            {
                Name = "tointeger",
                Func = ToInt
            }
        };

        public static int OpenLib(ILuaState ls)
        {
            ls.NewLib(mathLib);
            ls.PushNumber(Math.PI);
            ls.SetField(-2, "pi");
            ls.PushNumber(LuaFloat.MaxValue);
            ls.SetField(-2, "huge");
            ls.PushInteger(LuaInt.MaxValue);
            ls.SetField(-2, "maxinteger");
            ls.PushInteger(LuaInt.MinValue);
            ls.SetField(-2, "mininteger");
            ls.SetGlobal("math");
            return 0;
        }
    }
}