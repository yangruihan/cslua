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
                var proto = ProtoType.Undump(data);
                LuaMain(proto);
            }
        }

        private static void LuaMain(ProtoType proto)
        {
            var nRegs = (int) proto.MaxStackSize;
            var ls = new LuaState(nRegs + 8, proto);
            ls.SetTop(nRegs);
            for (;;)
            {
                var pc = (UInt32) ls.PC();
                var inst = new Instruction(ls.Fetch());
                if (inst.Opcode() != EOpCode.OP_RETURN)
                {
                    inst.Execute(ls);
                    Console.Write($"[{pc + 1:00}] {inst.OpName()}");
                    PrintStack(ls);
                }
                else
                {
                    break;
                }
            }
        }

        private static void PrintStack(LuaState ls)
        {
            var top = ls.GetTop();

            for (var i = 1; i <= top; i++)
            {
                var t = ls.Type(i);
                switch (t)
                {
                    case ELuaType.Boolean:
                        Console.Write($"[{ls.ToBoolean(i)}]");
                        break;

                    case ELuaType.Number:
                        Console.Write($"[{ls.ToNumber(i)}]");
                        break;

                    case ELuaType.String:
                        Console.Write($"[\"{ls.ToString(i)}\"]");
                        break;

                    default:
                        Console.Write($"[{ls.TypeName(t)}]");
                        break;
                }
            }

            Console.WriteLine();
        }
    }
}