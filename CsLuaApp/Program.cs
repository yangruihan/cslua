using System;
using System.IO;
using System.Text;
using CsLua.API;
using CsLua.Common;
using CsLua.Libs;

namespace CsLua
{
    internal class Program
    {
        private static readonly string EOFMARK = "<eof>";
        private static readonly string LUA_PROMPT = "> ";
        private static readonly string LUA_PROMPT2 = ">> ";

        private static string ProgName = "lua";

        private static byte[] GetBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        private static string ToStr(byte[] str)
        {
            return Encoding.UTF8.GetString(str);
        }

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

                return (int)EErrorCode.Ok;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                return (int)EErrorCode.Undefine;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return (int)EErrorCode.ErrRun;
            }
        }

        private static string GetPrompt(ILuaState l, bool firstLine)
        {
            l.GetGlobal(firstLine ? "_PROMPT" : "_PROMPT2");
            string p = l.ToString(-1) ??
                       (firstLine ? LUA_PROMPT : LUA_PROMPT2);
            return p;
        }

        private static bool PushLine(ILuaState l, bool firstLine)
        {
            var prmt = GetPrompt(l, firstLine);
            Console.Write(prmt);
            try
            {
                var line = Console.ReadLine();
                if (line == null) // no input
                    return false;

                l.Pop(1); // remove prompt
                var len = line.Length;
                if (len > 0 && line[len - 1] == '\n') // line ends with newline?
                    line = line.Substring(0, --len); // remove it

                if (firstLine && line[0] == '=') // compatibility with 5.2
                {
                    l.PushString(
                        $"return {line.Substring(1)}"); // change '=' to 'return'
                }
                else
                {
                    l.PushString(line);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static EErrorCode AddReturn(ILuaState l)
        {
            var line = l.ToString(-1);
            var retLine = l.PushString($"return {line}");
            var status = l.Load(GetBytes(retLine), "=stdin", "bt");
            if (status == EErrorCode.Ok)
            {
                l.Remove(-2); // remove modified line
            }
            else
            {
                l.Pop(2); // pop result from 'luaL_loadbuffer' and modified line
            }

            return status;
        }

        private static bool Incomplete(ILuaState l, EErrorCode status)
        {
            if (status == EErrorCode.ErrSyntax)
            {
                string msg = l.ToString(-1);
                if (msg.EndsWith(EOFMARK))
                {
                    l.Pop(1);
                    return true;
                }
            }

            return false;
        }

        private static EErrorCode MultiLine(ILuaState l)
        {
            while (true) // repeat until gets a complete statement
            {
                var line = l.ToString(1); // get what it has
                var status = l.Load(GetBytes(line), "=stdin", "bt"); // try it
                if (!Incomplete(l, status) || !PushLine(l, false))
                    return
                        status; // cannot or should not try to add continuation line

                l.PushString("\n"); // add newline ...
                l.Insert(-2); // ...between the two lines
                l.Concat(3); // join them
            }
        }

        private static EErrorCode LoadLine(ILuaState l)
        {
            l.SetTop(0);
            if (!PushLine(l, true))
                return EErrorCode.Undefine;

            EErrorCode status;
            if ((status = AddReturn(l)) != EErrorCode.Ok)
            {
                status =
                    MultiLine(
                        l); // try as command, maybe with continuation lines
            }

            l.Remove(1); // remove line from the stack
            l.Assert(l.GetTop() == 1);
            return status;
        }

        private static int MsgHandler(ILuaState l)
        {
            var msg = l.ToString(1);
            if (msg == null) // is error object not a string?
            {
                if (l.CallMeta(1, "__tostring") // does it have a metamethod 
                    && l.Type(-1) == ELuaType.String)
                {
                    return 1;
                }
                else
                {
                    msg = l.PushString(
                        $"(error object is a {l.TypeName(1)} value)");
                }
            }

            // TODO traceback
            return 1;
        }

        private static EErrorCode DoCall(ILuaState l, int nArg,
            int nRes)
        {
            var @base = l.GetTop() - nArg;
            l.PushCSFunction(MsgHandler);
            l.Insert(@base);
            var status = l.PCall(nArg, nRes, @base);
            l.Remove(@base);
            return status;
        }

        private static void Message(string progName, string msg)
        {
            if (!string.IsNullOrEmpty(progName))
                Console.Error.Write($"{progName}: ");
            Console.Error.Write($"{msg}\n");
        }

        private static void Print(ILuaState l)
        {
            var n = l.GetTop();
            if (n > 0)
            {
                l.CheckStack(LuaConst.LUA_MINSTACK, "too many results to print");
                l.GetGlobal("print");
                l.Insert(1);
                if (l.PCall(n, 0, 0) != EErrorCode.Ok)
                {
                    Message(ProgName,
                        l.PushString(
                            $"error calling 'print' ({l.ToString(-1)})"));
                }
            }
        }

        private static EErrorCode Report(ILuaState l, EErrorCode status)
        {
            if (status != EErrorCode.Ok)
            {
                var msg = l.ToString(-1);
                Message(ProgName, msg);
                l.Pop(1); // remove message
            }

            return status;
        }

        private static void DoRepl()
        {
            var l = CSLua.CreateLuaState();
            LoadLibs(l);

            EErrorCode status;
            var oldProgName = ProgName;
            ProgName = null;
            while ((status = LoadLine(l)) != EErrorCode.Undefine)
            {
                if (status == EErrorCode.Ok)
                    status = DoCall(l, 0, -1);

                if (status == EErrorCode.Ok)
                    Print(l);
                else
                    Report(l, status);
            }

            l.SetTop(0); // clear stack
            Console.WriteLine();
            ProgName = oldProgName;
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