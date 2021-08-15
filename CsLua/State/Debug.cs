using CsLua.Misc;
using CsLua.Binchunk;

namespace CsLua.State
{
    internal partial class LuaState
    {
        private static class _Debug
        {
            public static string UpvalName(LuaState l, ProtoType p, int uv)
            {
                l.Check(uv < p.Upvalues.Length, "");
                string name = p.UpvalueNames[uv];
                return name ?? "?";
            }

            public static string? GetUpvalName(LuaState l, CallInfo ci, LuaValue o, out string name)
            {
                LuaClosure c = l.Index2Addr(ci.Func)!.GetLuaClosureValue()!;
                for (int i = 0; i < c.Upvals!.Length; i++)
                {
                    if (c.Upvals[i].Val == o)
                    {
                        name = UpvalName(l, c.Proto, i);
                        return "upvalue";
                    }
                }

                name = "";
                return null;
            }

            public static string? GetObjName(LuaState l, ProtoType p, int lastPc, int reg, out string name)
            {
                // TODO
                name = "";
                return null;
            }

            public static bool IsInStack(CallInfo ci, int idx)
            {
                return (0 <= idx && idx < (ci.Top - ci.LuaClosure.Base));
            }

            public static int CurrentPc(LuaState l, CallInfo ci)
            {
                Debug.Assert(ci.IsLua());
                return l.PcRel(ci.LuaClosure.SavedPc);
            }

            public static int CurrentLine(LuaState l, CallInfo ci)
            {
                return l.GetFuncLine(l.Index2Addr(ci.Func)!.GetLuaClosureValue()!.Proto, CurrentPc(l, ci));
            }

            public static string VarInfo(LuaState l, int idx)
            {
                CallInfo ci = l.CallInfo;
                string? kind = null;
                string name = "";
                if (ci.IsLua())
                {
                    var o = l.Index2Addr(idx)!;
                    kind = GetUpvalName(l, ci, o, out name); // check whether 'o' is an upvalue
                    if (kind == null && IsInStack(ci, idx)) // no? try a register
                    {
                        var luaClosure = l.Index2Addr(ci.Func)!.GetLuaClosureValue()!;
                        kind = GetObjName(l, luaClosure.Proto,
                                          CurrentPc(l, ci),
                                          idx + ci.Func - ci.LuaClosure.Base,
                                          out name);
                    }
                }
                return kind != null ? $"({kind} '{name}')" : "";
            }
        }

        private int PcRel(int savedPc)
        {
            return savedPc - 1;
        }

        private int GetFuncLine(ProtoType p, int pc)
        {
            return p.LineInfo == null ? -1 : (int)p.LineInfo[pc];
        }

        private string AddInfo(string msg, string src, int line)
        {
            var buff = "";
            if (!string.IsNullOrEmpty(src))
            {
                // TODO add chunkid
            }
            else
            {
                buff = "?";
            }

            return PushString($"{buff}:{line} {msg}");
        }

        private void ErrorMsg()
        {
            if (ErrFunc != 0)
            {
                var errFunc = Index2Addr(ErrFunc)!;
                // [..., ErrFunc, Msg]
                Push(errFunc);
                Insert(-2);
                CallNoYield(-2, 1);
            }

            var msg = ToString(-1);
            Throw(API.EStatus.ErrRun, msg!);
        }

        private void RunError(string msg)
        {
            PushString(msg);
            if (CallInfo.IsLua())
            {
                AddInfo(msg, Index2Addr(CallInfo.Func)!.GetLuaClosureValue()!.Proto.Source, _Debug.CurrentLine(this, CallInfo));
            }
            ErrorMsg();
        }

        private void RunError(string fmt, params object[] args)
        {
            RunError(string.Format(fmt, args));
        }

        private void TypeError(int idx, string op)
        {
            var o = Index2Addr(idx)!;
            string t = ObjTypeName(o);
            RunError($"attempt to {op} a {t} value{_Debug.VarInfo(this, idx)}");
        }
    }
}