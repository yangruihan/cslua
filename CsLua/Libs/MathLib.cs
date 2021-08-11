using System;
using CsLua.API;
using CsLua.Number;

namespace CsLua.Libs
{
    using LuaInt = System.Int64;
    using LuaUInt = System.UInt64;
    using LuaFloat = System.Double;

    public static class MathLib
    {
        private static Random _csRandom;

        private static Random CsRandom => _csRandom ?? (_csRandom = new Random());

        private static void PushNumInt(ILuaState ls, LuaFloat d)
        {
            if (LuaMath.FloatToInteger(d, out var n))
                ls.PushNumber(n);
            else
                ls.PushNumber(d);
        }

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
            var y = ls.CheckNumber(1);
            var x = ls.OptNumber(2, 1);
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

        private static readonly CSFunction Floor = ls =>
        {
            if (ls.IsInteger(1))
            {
                ls.SetTop(1);
            }
            else
            {
                var d = Math.Floor(ls.CheckNumber(1));
                PushNumInt(ls, d);
            }

            return 1;
        };

        private static readonly CSFunction Ceil = ls =>
        {
            if (ls.IsInteger(1))
            {
                ls.SetTop(1);
            }
            else
            {
                ls.PushInteger((long) Math.Ceiling(ls.CheckNumber(1)));
            }

            return 1;
        };

        private static readonly CSFunction FMod = ls =>
        {
            if (ls.IsInteger(1) && ls.IsInteger(2))
            {
                var d = ls.ToInteger(2);
                if ((LuaUInt) d + 1u <= 1u) /* special cases: -1 or 0 */
                {
                    if (d == 0)
                        ls.Error("div zero");
                    else
                        ls.PushInteger(ls.ToInteger(1) % d);
                }
            }
            else
            {
                ls.PushNumber(ls.CheckNumber(1) % ls.CheckNumber(2));
            }

            return 1;
        };

        private static readonly CSFunction Modf = ls =>
        {
            if (ls.IsInteger(1))
            {
                ls.SetTop(1);
                ls.PushNumber(0);
            }
            else
            {
                var n = ls.CheckNumber(1);
                var ip = (n < 0) ? Math.Ceiling(n) : Math.Floor(n);
                PushNumInt(ls, ip);
                ls.PushNumber(Math.Abs(n - ip) <= 0 ? 0.0 : n - ip);
            }

            return 2;
        };

        private static readonly CSFunction Sqrt = ls =>
        {
            ls.PushNumber(Math.Sqrt(ls.CheckNumber(1)));
            return 1;
        };

        private static readonly CSFunction Ult = ls =>
        {
            var a = ls.CheckInteger(1);
            var b = ls.CheckInteger(2);
            ls.PushBoolean((LuaUInt) a < (LuaUInt) b);
            return 1;
        };

        private static readonly CSFunction Log = ls =>
        {
            var x = ls.CheckNumber(1);
            LuaFloat res;
            if (ls.IsNoneOrNil(2))
                res = Math.Log(x);
            else
            {
                var @base = ls.CheckNumber(2);
                res = Math.Log(x, @base);
            }

            ls.PushNumber(res);
            return 1;
        };

        private static readonly CSFunction Exp = ls =>
        {
            ls.PushNumber(Math.Exp(ls.CheckNumber(1)));
            return 1;
        };

        private static readonly CSFunction Deg = ls =>
        {
            ls.PushNumber(ls.CheckNumber(1) * 180.0 / Math.PI);
            return 1;
        };

        private static readonly CSFunction Rad = ls =>
        {
            ls.PushNumber(ls.CheckNumber(1) * Math.PI / 180.0);
            return 1;
        };

        private static readonly CSFunction Min = ls =>
        {
            var n = ls.GetTop();
            var imin = 1;
            ls.ArgCheck(n >= 1, 1, "value expected");

            for (var i = 2; i <= n; i++)
            {
                if (ls.Compare(i, imin, ECompOp.Lt))
                    imin = i;
            }

            ls.PushValue(imin);
            return 1;
        };

        private static readonly CSFunction Max = ls =>
        {
            var n = ls.GetTop();
            var imax = 1;
            ls.ArgCheck(n >= 1, 1, "value expected");

            for (var i = 2; i <= n; i++)
            {
                if (ls.Compare(imax, i, ECompOp.Lt))
                    imax = i;
            }

            ls.PushValue(imax);
            return 1;
        };

        private static readonly CSFunction Random = ls =>
        {
            var r = CsRandom.NextDouble();
            LuaInt low = 0, up = 0;

            switch (ls.GetTop())
            {
                case 0:
                    ls.PushNumber(r);
                    break;

                case 1:
                    low = 1;
                    up = ls.CheckInteger(1);
                    break;

                case 2:
                    low = ls.CheckInteger(1);
                    up = ls.CheckInteger(2);
                    break;

                default:
                    ls.Error("wrong number of arguments");
                    break;
            }

            ls.ArgCheck(low <= up, 1, "interval is empty");
            ls.ArgCheck(low >= 0 || up <= Consts.LUA_MAXINTEGER + low, 1, "interval too large");

            r *= (LuaFloat) (up - low) + 1.0f;
            ls.PushInteger((LuaInt) r + low);
            return 1;
        };

        private static readonly CSFunction RandomSeed = ls =>
        {
            _csRandom = new Random((int) (LuaInt) ls.CheckNumber(1));
            _csRandom.Next();
            return 0;
        };

        private static readonly CSFunction Type = ls =>
        {
            if (ls.Type(1) == ELuaType.Number)
            {
                if (ls.IsInteger(1))
                    ls.PushString("integer");
                else
                    ls.PushString("float");
            }
            else
            {
                ls.CheckAny(1);
                ls.PushNil();
            }

            return 1;
        };

        private static readonly LuaReg[] mathLib =
        {
            new LuaReg()
            {
                Name = "abs",
                Func = Abs
            },
            new LuaReg()
            {
                Name = "acos",
                Func = Acos
            },
            new LuaReg()
            {
                Name = "asin",
                Func = Asin
            },
            new LuaReg()
            {
                Name = "atan",
                Func = Atan
            },
            new LuaReg()
            {
                Name = "ceil",
                Func = Ceil
            },
            new LuaReg()
            {
                Name = "cos",
                Func = Cos
            },
            new LuaReg()
            {
                Name = "deg",
                Func = Deg,
            },
            new LuaReg()
            {
                Name = "exp",
                Func = Exp
            },
            new LuaReg()
            {
                Name = "tointeger",
                Func = ToInt
            },
            new LuaReg()
            {
                Name = "floor",
                Func = Floor
            },
            new LuaReg()
            {
                Name = "fmod",
                Func = FMod
            },
            new LuaReg()
            {
                Name = "ult",
                Func = Ult
            },
            new LuaReg()
            {
                Name = "log",
                Func = Log
            },
            new LuaReg()
            {
                Name = "max",
                Func = Max,
            },
            new LuaReg()
            {
                Name = "min",
                Func = Min,
            },
            new LuaReg()
            {
                Name = "modf",
                Func = Modf
            },
            new LuaReg()
            {
                Name = "rad",
                Func = Rad
            },
            new LuaReg()
            {
                Name = "random",
                Func = Random
            },
            new LuaReg()
            {
                Name = "randomseed",
                Func = RandomSeed
            },
            new LuaReg()
            {
                Name = "sin",
                Func = Sin
            },
            new LuaReg()
            {
                Name = "sqrt",
                Func = Sqrt
            },
            new LuaReg()
            {
                Name = "tan",
                Func = Tan
            },
            new LuaReg()
            {
                Name = "type",
                Func = Type
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