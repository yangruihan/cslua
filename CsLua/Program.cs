using System;
using System.IO;
using CsLua.API;
using CsLua.Binchunk;
using CsLua.State;
using CsLua.VM;

namespace CsLua
{
    internal class Program
    {
        private static CSFunction GetMetaTable = ls =>
        {
            if (!ls.GetMetaTable(1))
            {
                ls.PushNil();
            }

            return 1;
        };

        private static CSFunction SetMetaTable = ls =>
        {
            ls.SetMetaTable(1);
            return 1;
        };

        private static CSFunction Next = ls =>
        {
            ls.SetTop(2);
            if (ls.Next(1))
            {
                return 2;
            }

            ls.PushNil();
            return 1;
        };

        private static CSFunction Pairs = ls =>
        {
            ls.PushCSFunction(Next);
            ls.PushValue(1);
            ls.PushNil();
            return 3;
        };

        private static CSFunction IPairs = ls =>
        {
            ls.PushCSFunction(IPairsAux);
            ls.PushValue(1);
            ls.PushInteger(0);
            return 3;
        };

        private static CSFunction Print = ls =>
        {
            var nArgs = ls.GetTop();
            for (var i = 1; i <= nArgs; i++)
            {
                if (ls.IsBoolean(i))
                    Console.Write(ls.ToBoolean(i).ToString());
                else if (ls.IsString(i))
                    Console.Write(ls.ToString(i));
                else
                    Console.Write(ls.TypeName(ls.Type(i)));

                if (i < nArgs)
                    Console.Write("\t");
            }

            Console.WriteLine();
            return 0;
        };

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var data = File.ReadAllBytes(args[0]);

                var ls = new LuaState();
                ls.Register("print", Print);
                ls.Register("getmetatable", GetMetaTable);
                ls.Register("setmetatable", SetMetaTable);
                ls.Register("next", Next);
                ls.Register("pairs", Pairs);
                ls.Register("ipairs", IPairs);
                ls.Load(data, "chunk", "b");
                ls.Call(0, 0);
            }
        }

        private static int IPairsAux(ILuaState ls)
        {
            var i = ls.ToInteger(2) + 1;
            ls.PushInteger(i);
            return ls.GetI(1, i) == ELuaType.Nil ? 1 : 2;
        }
    }
}