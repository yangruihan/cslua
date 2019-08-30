using System;
using CsLua.API;
using CsLua.State;

namespace CsLua
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var ls = new LuaState();

            ls.PushInteger(1);
            ls.PushString("2.0");
            ls.PushString("3.0");
            ls.PushNumber(4.0);
            PrintStack(ls);

            ls.Arith(EArithOp.Add);
            PrintStack(ls);
            ls.Arith(EArithOp.BNot);
            PrintStack(ls);
            ls.Len(2);
            PrintStack(ls);
            ls.Concat(3);
            PrintStack(ls);
            ls.PushBoolean(ls.Compare(1, 2, ECompOp.Eq));
            PrintStack(ls);
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