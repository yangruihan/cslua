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
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var data = File.ReadAllBytes(args[0]);

                var ls = new LuaState();
                ls.Load(data, args[0], "b");
                ls.Call(0, 0);
            }
        }
    }
}