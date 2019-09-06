using System;
using CsLua.API;

namespace CsLua.State
{
    partial class LuaState : ILuaState
    {
        public string TypeName(ELuaType tp)
        {
            switch (tp)
            {
                case ELuaType.None:
                    return "no value";
                case ELuaType.Nil:
                    return "nil";
                case ELuaType.Boolean:
                    return "boolean";
                case ELuaType.Number:
                    return "number";
                case ELuaType.String:
                    return "string";
                case ELuaType.Table:
                    return "table";
                case ELuaType.Function:
                    return "function";
                case ELuaType.Thread:
                    return "thread";
                default:
                    return "userdata";
            }
        }

        public ELuaType Type(int idx)
        {
            if (_stack.IsValid(idx))
            {
                var val = _stack[idx];
                return val.Type();
            }

            return ELuaType.None;
        }

        public bool IsNone(int idx)
        {
            return Type(idx) == ELuaType.None;
        }

        public bool IsNil(int idx)
        {
            return Type(idx) == ELuaType.Nil;
        }

        public bool IsNoneOrNil(int idx)
        {
            return IsNone(idx) || IsNil(idx);
        }

        public bool IsBoolean(int idx)
        {
            return Type(idx) == ELuaType.Boolean;
        }

        public bool IsTable(int idx)
        {
            return Type(idx) == ELuaType.Table;
        }

        public bool IsFunction(int idx)
        {
            return Type(idx) == ELuaType.Function;
        }

        public bool IsThread(int idx)
        {
            return Type(idx) == ELuaType.Thread;
        }

        public bool IsString(int idx)
        {
            var t = Type(idx);
            return t == ELuaType.String || t == ELuaType.Number;
        }

        public bool IsNumber(int idx)
        {
            return ToNumberX(idx, out _);
        }

        public bool IsInteger(int idx)
        {
            var val = _stack[idx];
            return val.Value is Int64;
        }

        public bool ToBoolean(int idx)
        {
            var val = _stack[idx];
            return val.ToBoolean();
        }

        public long ToInteger(int idx)
        {
            ToIntegerX(idx, out var ret);
            return ret;
        }

        public bool ToIntegerX(int idx, out long ret)
        {
            var val = _stack[idx];
            var ok = val.Value is Int64;
            ret = ok ? (Int64) val.Value : 0;
            return ok;
        }

        public double ToNumber(int idx)
        {
            ToNumberX(idx, out var ret);
            return ret;
        }

        public bool ToNumberX(int idx, out double ret)
        {
            var val = _stack[idx];
            if (val.Value is double)
            {
                ret = (double) val.Value;
                return true;
            }

            if (val.Value is Int64)
            {
                ret = (Int64) val.Value;
                return true;
            }

            ret = 0;
            return false;
        }

        public string ToString(int idx)
        {
            ToStringX(idx, out var ret);
            return ret;
        }

        public bool ToStringX(int idx, out string ret)
        {
            var val = _stack[idx];
            if (val.Value is string)
            {
                ret = (string) val.Value;
                return true;
            }

            if (val.Value is Int64 || val.Value is double)
            {
                ret = val.Value.ToString();
                _stack[idx] = new LuaValue(ret);
                return true;
            }

            ret = "";
            return false;
        }

        public bool IsCSFunction(int idx)
        {
            var val = _stack[idx];
            if (val.Value is Closure c)
                return c.CSFunction != null;
            return false;
        }

        public CSFunction ToCSFunction(int idx)
        {
            var val = _stack[idx];
            if (val.Value is Closure c)
                return c.CSFunction;
            return null;
        }
    }
}