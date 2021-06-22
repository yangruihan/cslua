using System;
using System.IO;
using CsLua.API;
using CsLua.Common;
using CsLua.Libs;

namespace CsLua
{
    internal class Program
    {
        private static void LoadLibs(ILuaState ls)
        {
            BaseLib.OpenLib(ls);
            MathLib.OpenLib(ls);
        }
        
        private static int DoFile(string filePath)
        {
            try
            {
                var data = File.ReadAllBytes(filePath);

                var l = CSLua.CreateLuaState();
                LoadLibs(l);
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
            var l = CSLua.CreateLuaState();
            LoadLibs(l);

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