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
        public static CSFunction GetMetaTable = ls =>
        {
            if (!ls.GetMetaTable(1))
            {
                ls.PushNil();
            }

            return 1;
        };

        public static CSFunction SetMetaTable = ls =>
        {
            ls.SetMetaTable(1);
            return 1;
        };

        public static CSFunction Print = ls =>
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
                ls.Load(data, "chunk", "b");
                ls.Call(0, 0);
            }
        }
    }
}