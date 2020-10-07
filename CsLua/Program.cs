using System;
using System.IO;
using CsLua.API;
using CsLua.Common;
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

        private static int InitSystemLib(LuaState l)
        {
            l.Register("print", Print);
            l.Register("getmetatable", GetMetaTable);
            l.Register("setmetatable", SetMetaTable);
            l.Register("next", Next);
            l.Register("pairs", Pairs);
            l.Register("ipairs", IPairs);
            l.Register("error", Error);
            l.Register("pcall", PCall);

            return (int) EErrorCode.Ok;
        }

        private static int DoFile(string filePath)
        {
            try
            {
                var data = File.ReadAllBytes(filePath);

                var l = new LuaState();
                InitSystemLib(l);
                l.Load(data, filePath, "bt");
                l.Call(0, 0);

                return (int) EErrorCode.Ok;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                return (int) EErrorCode.ErrFile;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return (int) EErrorCode.ErrRun;
            }
        }

        private static int DoRepl()
        {
            var l = new LuaState();
            InitSystemLib(l);

            while (true)
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    continue;

                try
                {
                    l.Load(line.GetBytes(), "repl", "bt");
                    l.Call(0, 0);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length > 0)
                DoFile(args[0]);
            else
                DoRepl();
        }
    }
}