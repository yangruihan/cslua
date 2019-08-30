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
            
            ls.PushBoolean(true);
            PrintStack(ls);
            ls.PushInteger(10);
            PrintStack(ls);
            ls.PushNil();
            PrintStack(ls);
            ls.PushString("hello");
            PrintStack(ls);
            ls.PushValue(-4);
            PrintStack(ls);
            ls.Replace(3);
            PrintStack(ls);
            ls.SetTop(6);
            PrintStack(ls);
            ls.Remove(-3);
            PrintStack(ls);
            ls.SetTop(-5);
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