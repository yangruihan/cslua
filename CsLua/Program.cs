using System;
using System.Globalization;
using System.IO;
using CsLua.Binchunk;
using CsLua.VM;

namespace CsLua
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    var data = File.ReadAllBytes(args[0]);
                    var proto = ProtoType.Undump(data);
                    if (proto != null)
                        List(proto);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[CsLua Error] {e}");
                }
            }
        }

        private static void List(ProtoType f)
        {
            PrintHeader(f);
            PrintCode(f);
            PrintDetail(f);
            foreach (var protoType in f.Protos)
            {
                List(protoType);
            }
        }

        private static void PrintDetail(ProtoType f)
        {
            Console.Write($"constants ({f.Constatns.Length}):\n");
            for (var i = 0; i < f.Constatns.Length; i++)
            {
                var k = f.Constatns[i];
                Console.Write($"\t{i + 1}\t{ConstantToString(k)}\n");
            }

            Console.Write($"locals ({f.LocVars.Length}):\n");
            for (var i = 0; i < f.LocVars.Length; i++)
            {
                var locVar = f.LocVars[i];
                Console.Write($"\t{i}\t{locVar.VarName}\t{locVar.StartPC + 1}\t{locVar.EndPC + 1}");
            }

            Console.Write($"upvalues ({f.Upvalues.Length}):\n");
            for (var i = 0; i < f.Upvalues.Length; i++)
            {
                var upval = f.Upvalues[i];
                Console.Write($"\t{i}\t{UpvalName(f, i)}\t{upval.Instack}\t{upval.Idx}\n");
            }
        }

        private static string ConstantToString(object o)
        {
            if (o is null)
                return "nil";

            var t = o.GetType();
            if (t == typeof(bool))
            {
                return (bool) o ? "true" : "false";
            }

            if (t == typeof(double))
            {
                return ((double) o).ToString(CultureInfo.InvariantCulture);
            }

            if (t == typeof(Int64))
            {
                return $"\"{((Int64) o).ToString()}\"";
            }

            if (t == typeof(string))
            {
                return (string) o;
            }

            return "?";
        }

        private static string UpvalName(ProtoType f, int idx)
        {
            if (f.UpvalueNames.Length > 0)
            {
                return f.UpvalueNames[idx];
            }

            return "-";
        }


        private static void PrintCode(ProtoType f)
        {
            for (var pc = 0; pc < f.Code.Length; pc++)
            {
                var c = f.Code[pc];
                var line = "-";
                if (f.LineInfo.Length > 0)
                    line = f.LineInfo[pc].ToString();

                var i = new Instruction(c);
                Console.Write($"\t{pc + 1}\t[{line}]\t{i.OpName()} \t");
                PrintOperands(i);
                Console.WriteLine();
            }
        }

        private static void PrintOperands(Instruction i)
        {
            int a, b, c;
            switch (i.OpMode())
            {
                case EOpMode.IABC:
                    i.ABC(out a, out b, out c);
                    Console.Write($"{a}");

                    if (i.BMode() != EOpArgMask.OpArgN)
                    {
                        if (b > 0xff)
                            Console.Write($" {-1 - (b & 0xff)}");
                        else
                            Console.Write($" {b}");
                    }

                    if (i.CMode() != EOpArgMask.OpArgN)
                    {
                        if (c > 0xff)
                            Console.Write($" {-1 - (c & 0xff)}");
                        else
                            Console.Write($" {c}");
                    }

                    break;

                case EOpMode.IABx:
                    i.ABx(out a, out b);
                    Console.Write($"{a}");

                    if (i.BMode() == EOpArgMask.OpArgK)
                        Console.Write($" {-1 - b}");
                    else if (i.BMode() == EOpArgMask.OpArgU)
                        Console.Write($" {b}");

                    break;

                case EOpMode.IAsBx:
                    i.AsBx(out a, out b);
                    Console.Write($"{a} {b}");
                    break;

                case EOpMode.IAx:
                    i.Ax(out a);
                    Console.Write($"{-1 - a}");
                    break;
            }
        }

        private static void PrintHeader(ProtoType f)
        {
            var funcType = "main";
            if (f.LineDefined > 0)
                funcType = "function";

            var varargFlag = "";
            if (f.IsVararg > 0)
                varargFlag = "+";

            Console.Write($"\n{funcType} <{f.Source}:{f.LineDefined},{f.LastLineDefined}> ({f.Code.Length})\n");

            Console.Write($"{f.NumParams}{varargFlag}, {f.MaxStackSize} slots, {f.Upvalues.Length} upvalues, ");

            Console.Write($"{f.LocVars.Length} locals, {f.Constatns.Length} constants, {f.Protos.Length} functions\n");
        }
    }
}