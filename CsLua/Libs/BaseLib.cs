using System;
using CsLua.API;

namespace CsLua.Libs
{
    public static class BaseLib
    {
        private static readonly LuaCSFunction GetMetaTable = ls =>
        {
            if (!ls.GetMetaTable(1))
            {
                ls.PushNil();
            }

            return 1;
        };

        private static readonly LuaCSFunction SetMetaTable = ls =>
        {
            ls.SetMetaTable(1);
            return 1;
        };

        private static readonly LuaCSFunction RawGet = ls =>
        {
            ls.PushValue(-2); // table
            ls.PushValue(-2); // key
            ls.RawGet(-2);
            return 1;
        };

        private static readonly LuaCSFunction RawSet = ls =>
        {
            ls.PushValue(-3); // table
            ls.PushValue(-3); // key
            ls.PushValue(-3); // value
            ls.RawSet(-3);
            return 0;
        };

        private static readonly LuaCSFunction Next = ls =>
        {
            ls.SetTop(2);
            if (ls.Next(1))
            {
                return 2;
            }

            ls.PushNil();
            return 1;
        };

        private static readonly LuaCSFunction Pairs = ls =>
        {
            ls.PushCSFunction(Next);
            ls.PushValue(1);
            ls.PushNil();
            return 3;
        };

        private static readonly LuaCSFunction IPairs = ls =>
        {
            ls.PushCSFunction(IPairsAux);
            ls.PushValue(1);
            ls.PushInteger(0);
            return 3;
        };

        private static readonly LuaCSFunction Print = ls =>
        {
            var nArgs = ls.GetTop();
            for (var i = 1; i <= nArgs; i++)
            {
                if (ls.IsBoolean(i))
                    Console.Write(ls.ToBoolean(i).ToString());
                else if (ls.IsString(i))
                    Console.Write(ls.ToString(i));
                else if (ls.IsInteger(i))
                    Console.Write(ls.ToInteger(i));
                else if (ls.IsNumber(i))
                    Console.Write(ls.ToNumber(i));
                else
                    Console.Write(ls.TypeName(ls.Type(i)));

                if (i < nArgs)
                    Console.Write("\t");
            }

            Console.WriteLine();
            return 0;
        };

        private static readonly LuaCSFunction Error = ls => ls.Error();

        private static readonly LuaCSFunction PCall = ls =>
        {
            var nArgs = ls.GetTop() - 1;
            var status = ls.PCall(nArgs, -1, 0);
            ls.PushBoolean(status == (int) EErrorCode.Ok);
            ls.Insert(1);
            return ls.GetTop();
        };

        private static readonly LuaCSFunction IPairsAux = ls =>
        {
            var i = ls.ToInteger(2) + 1;
            ls.PushInteger(i);
            return ls.GetI(1, i) == ELuaType.Nil ? 1 : 2;
        };

        public static int OpenLib(ILuaState ls)
        {
            ls.Register("print", Print);
            ls.Register("getmetatable", GetMetaTable);
            ls.Register("setmetatable", SetMetaTable);
            ls.Register("rawget", RawGet);
            ls.Register("rawset", RawSet);
            ls.Register("next", Next);
            ls.Register("pairs", Pairs);
            ls.Register("ipairs", IPairs);
            ls.Register("error", Error);
            ls.Register("pcall", PCall);
            return 0;
        }
    }
}