using System;
using System.IO;
using CsLua.API;
using CsLua.Compiler.Lexer;
using CsLua.Compiler.Parser;
using CsLua.State;

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

        private static CSFunction Error = ls => ls.Error();

        private static CSFunction PCall = ls =>
        {
            var nArgs = ls.GetTop() - 1;
            var status = ls.PCall(nArgs, -1, 0);
            ls.PushBoolean(status == (int) EErrorCode.Ok);
            ls.Insert(1);
            return ls.GetTop();
        };

        private static CSFunction IPairsAux = ls =>
        {
            var i = ls.ToInteger(2) + 1;
            ls.PushInteger(i);
            return ls.GetI(1, i) == ELuaType.Nil ? 1 : 2;
        };

        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var data = File.ReadAllText(args[0]);
                TestLexer(data, args[0]);

//                var data = File.ReadAllBytes(args[0]);
//
//                var ls = new LuaState();
//                ls.Register("print", Print);
//                ls.Register("getmetatable", GetMetaTable);
//                ls.Register("setmetatable", SetMetaTable);
//                ls.Register("next", Next);
//                ls.Register("pairs", Pairs);
//                ls.Register("ipairs", IPairs);
//                ls.Register("error", Error);
//                ls.Register("pcall", PCall);
//                ls.Load(data, "chunk", "b");
//                ls.Call(0, 0);
            }
        }

        private static void TestLexer(string chunk, string chunkName)
        {
            var ast = Parser.Parse(chunk, chunkName);
            ast.Print(0);
        }
    }
}