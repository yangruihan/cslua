using CsLua.API;

namespace CsLua.State
{
    /// <summary>
    /// 从栈里获取信息
    /// </summary>
    internal partial class LuaState : ILuaState
    {
        public uint RawLen(int idx)
        {
            var val = Stack[idx];
            if (val.IsString())
                return (uint) val.GetStrValue().Length;

            if (val.IsTable())
                return (uint) val.GetTableValue().Len();

            return 0;
        }

        /// <summary>
        /// 把给定Lua类型转换成对应的字符串表示
        /// </summary>
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
                case ELuaType.Int:
                    return "integer";
                case ELuaType.String:
                    return "string";
                case ELuaType.Table:
                    return "table";
                case ELuaType.Function:
                    return "function";
                case ELuaType.CSFunction:
                    return "csfunction";
                case ELuaType.Thread:
                    return "thread";
                default:
                    return "userdata";
            }
        }

        /// <summary>
        /// 根据索引返回值的类型，如果索引无效，则返回 LUA_TNONE
        /// </summary>
        public ELuaType Type(int idx)
        {
            if (Stack.IsValid(idx))
            {
                var val = Stack[idx];
                return val.Type;
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
            return Type(idx).IsFunction();
        }

        public bool IsThread(int idx)
        {
            return Type(idx) == ELuaType.Thread;
        }

        public bool IsString(int idx)
        {
            var t = Type(idx);
            return t == ELuaType.String || t.IsNumber();
        }

        public bool IsNumber(int idx)
        {
            return ToNumberX(idx, out _);
        }

        public bool IsInteger(int idx)
        {
            return Type(idx) == ELuaType.Int;
        }

        public bool IsCSFunction(int idx)
        {
            var val = Stack[idx];
            return val.IsCSFunction();
        }

        public bool IsUserdata(int idx)
        {
            return Type(idx).IsUserdata();
        }

        public bool IsArray(int idx)
        {
            if (!IsTable(idx)) return false;
            return Stack[idx].GetTableValue().IsArray();
        }

        public bool ToBoolean(int idx)
        {
            var val = Stack[idx];
            return val.ToBoolean();
        }

        public LuaInt ToInteger(int idx)
        {
            ToIntegerX(idx, out var ret);
            return ret;
        }

        public bool ToIntegerX(int idx, out LuaInt ret)
        {
            var val = Stack[idx];
            var ok = val.IsInt();
            ret = ok ? val.GetIntValue() : 0;
            return ok;
        }

        public LuaFloat ToNumber(int idx)
        {
            ToNumberX(idx, out var ret);
            return ret;
        }

        public bool ToNumberX(int idx, out LuaFloat ret)
        {
            var val = Stack[idx];
            if (val.IsFloat())
            {
                ret = val.GetFloatValue();
                return true;
            }

            if (val.IsInt())
            {
                ret = val.GetIntValue();
                return true;
            }

            ret = 0;
            return false;
        }

        public string ToString(int idx)
        {
            if (ToStringX(idx, out var ret))
                return ret;
            return null;
        }

        public bool ToStringX(int idx, out string ret)
        {
            var val = Stack[idx];
            if (val.IsString())
            {
                ret = val.GetStrValue();
                return true;
            }

            if (val.IsNumber())
            {
                ret = val.ToString();
                Stack[idx] = new LuaValue(ret, ELuaType.String);
                return true;
            }

            ret = "";
            return false;
        }

        public LuaCSFunction ToCSFunction(int idx)
        {
            var val = Stack[idx];
            return val.GetCSFunctionValue();
        }

        public object ToUserdata(int idx)
        {
            return Stack[idx].GetObjValue();
        }

        public ILuaState ToThread(int idx)
        {
            var val = Stack[idx];
            return val.IsThread() ? val.GetValue() as ILuaState : null;
        }

        public object ToPointer(int idx)
        {
            var val = Stack[idx];
            return val.GetObjValue();
        }
    }
}