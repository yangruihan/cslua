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

        private static readonly LuaReg[] mathLib = new[]
        {
            new LuaReg()
            {
                Name = "abs",
                Func = Abs
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