using System;
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
            var val = Index2Addr(idx)!;
            if (val.IsString())
                return (uint) val.GetStrValue().Length;

            if (val.IsTable())
                return (uint) val.GetTableValue().Len();

            if (val.IsUserData())
                return (uint) val.GetUserDataValue().Size;

            return 0;
        }

        /// <summary>
        /// 把给定Lua类型转换成对应的字符串表示
        /// </summary>
        public string TypeName(ELuaType tp)
        {
            Check(ELuaType.None <= tp && tp < ELuaType.NumTags, "invalid tag");

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
                case ELuaType.LCSFunction:
                    return "csfunction";
                case ELuaType.CSClosure:
                    return "csclosure";
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
            var v = Index2Addr(idx, out var absIdx);
            if (LuaAPI.IsValid(v))
                return v!.Type.GetNoVariantsType();

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
            var v = Index2Addr(idx);
            return v!.IsInt();
        }

        public bool IsCSFunction(int idx)
        {
            var val = Index2Addr(idx);
            return val!.IsLCSFunction() || val!.IsCSClosure();
        }

        public bool IsUserdata(int idx)
        {
            var v = Index2Addr(idx);
            return Type(idx).IsUserdata();
        }

        public bool IsArray(int idx)
        {
            if (!IsTable(idx)) return false;
            return Stack[idx].GetTableValue().IsArray();
        }

        public bool ToBoolean(int idx)
        {
            var val = Index2Addr(idx)!;
            return val.ToBoolean();
        }

        public LuaInt ToInteger(int idx)
        {
            ToIntegerX(idx, out var ret);
            return ret;
        }

        public bool ToIntegerX(int idx, out LuaInt ret)
        {
            var val = Index2Addr(idx)!;
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
            var val = Index2Addr(idx)!;
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

        public string? ToString(int idx)
        {
            if (ToStringX(idx, out var ret))
                return ret;
            return null;
        }

        public bool ToStringX(int idx, out string ret)
        {
            var val = Index2Addr(idx, out var absIdx)!;
            if (val.IsString())
            {
                ret = val.GetStrValue();
                return true;
            }

            if (val.IsNumber())
            {
                ret = val.ToString();
                Stack[absIdx] = new LuaValue(ret, ELuaType.String);
                return true;
            }

            ret = "";
            return false;
        }

        public LuaCSFunction? ToCSFunction(int idx)
        {
            var val = Index2Addr(idx)!;
            if (val.IsLCSFunction())
                return val.GetLCSFunctionValue();
            else if (val.IsCSClosure())
                return val.GetCSClosureFunctionValue();
            return null;
        }

        public object? ToUserdata(int idx)
        {
            var v = Index2Addr(idx)!;
            switch (v.Type.GetNoVariantsType())
            {
                case ELuaType.UserData:
                    return v.GetUserDataValue().Memory;

                case ELuaType.LightUserData:
                    return v.GetObjValue();

                default:
                    return null;
            }
        }

        public ILuaState? ToThread(int idx)
        {
            var val = Index2Addr(idx)!;
            return val.IsThread() ? val.GetValue() as ILuaState : null;
        }

        public ref object ToPointer(int idx)
        {
            throw new NotImplementedException();
        }
    }
}