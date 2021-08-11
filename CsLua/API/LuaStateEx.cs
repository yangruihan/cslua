namespace CsLua.API
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    public static class LuaStateEx
    {
        // ----- Error -----
        private static void IntError(ILuaState ls, int arg)
        {
            if (ls.IsNumber(arg))
                ls.ArgError(arg, "number has no integer representation");
            else
                ls.TagError(arg, ELuaType.Number);
        }

        private static int TypeError(ILuaState ls, int arg, string typeName)
        {
            string typeArg;

            if (ls.GetMetaField(arg, "__name") == ELuaType.String)
                typeArg = ls.ToString(-1);
            else if (ls.Type(arg) == ELuaType.LightUserData)
                typeArg = "light userdata";
            else
                typeArg = ls.TypeName(arg);

            var msg = ls.PushString($"{typeName} expected, got {typeArg}");
            return ls.ArgError(arg, msg);
        }

        public static int Error(this ILuaState ls, string msg)
        {
            ls.PushString(msg);
            return ls.Error();
        }

        public static void TagError(this ILuaState ls, int arg, ELuaType type)
        {
            TypeError(ls, arg, ls.TypeName(type));
        }

        public static int ArgError(this ILuaState ls, int arg, string msg)
        {
            //TODO 完善代码
            return ls.Error($"bad argument #{arg} ({msg})");
        }

        public static int ArgCheck(this ILuaState ls, bool cond, int arg, string msg)
        {
            return !cond ? ls.ArgError(arg, msg) : 0;
        }

        public static void CheckStack(this ILuaState ls, int n, string errorMsg)
        {
            if (!ls.CheckStack(n))
                ls.Error(errorMsg);
        }

        public static LuaInt CheckInteger(this ILuaState ls, int arg)
        {
            if (!ls.ToIntegerX(arg, out var ret))
                IntError(ls, arg);

            return ret;
        }

        public static LuaFloat CheckNumber(this ILuaState ls, int arg)
        {
            if (!ls.ToNumberX(arg, out var ret))
                ls.TagError(arg, ELuaType.Number);

            return ret;
        }

        public static void CheckAny(this ILuaState ls, int arg)
        {
            if (ls.Type(arg) == ELuaType.None)
                ls.ArgError(arg, "value expected");
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

        // ----- 访问操作 -----

        public static string TypeName(this ILuaState ls, int arg)
        {
            return ls.TypeName(ls.Type(arg));
        }

        // ----- Metatable -----

        public static ELuaType GetMetaField(this ILuaState ls, int obj, string @event)
        {
            if (!ls.GetMetaTable(obj))
            {
                return ELuaType.Nil;
            }
            else
            {
                ls.PushString(@event);
                var type = ls.RawGet(-2);
                if (type == ELuaType.Nil)
                    ls.Pop(2);
                else
                    ls.Remove(-2);
                return type;
            }
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