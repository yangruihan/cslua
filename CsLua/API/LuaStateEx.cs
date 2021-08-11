namespace CsLua.API
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    public static class LuaStateEx
    {
        public static int Error(this ILuaState ls, string msg)
        {
            ls.PushString(msg);
            return ls.Error();
        }

        public static void CheckStack(this ILuaState ls, int n, string errorMsg)
        {
            if (!ls.CheckStack(n))
                ls.Error(errorMsg);
        }

        public static LuaFloat CheckNumber(this ILuaState ls, int arg)
        {
            if (!ls.ToNumberX(arg, out var ret))
                ls.Error($"Expected number, but {ls.TypeName(ls.Type(arg))}");

            return ret;
        }

        public static void CheckAny(this ILuaState ls, int arg)
        {
            if (ls.Type(arg) == ELuaType.None)
                ls.Error("value expected");
        }

        public static LuaFloat OptNumber(this ILuaState ls, int arg, LuaFloat defaultValue)
        {
            if (ls.IsNoneOrNil(arg))
                return defaultValue;
            else
                return ls.CheckNumber(arg);
        }

        public static void NewLib(this ILuaState ls, LuaReg[] lib)
        {
            ls.CreateTable(0, lib.Length);
            ls.SetFuncs(lib, 0);
        }

        public static void SetFuncs(this ILuaState ls, LuaReg[] lib, int nup)
        {
            ls.CheckStack(nup, "too many upvalues");

            // fill the table with given functions
            for (var i = 0; i < lib.Length; i++)
            {
                // copy upvalues to the top
                for (var j = 0; j < nup; j++)
                    ls.PushValue(-nup);

                // closure with those upvalues
                ls.PushCSClosure(lib[i].Func, nup);
                ls.SetField(-(nup + 2), lib[i].Name);
            }

            ls.Pop(nup); // remove upvalues
        }
    }
}