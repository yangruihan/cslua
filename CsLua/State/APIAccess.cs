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
            var val = GetValueByRelIdx(idx)!;
            if (val.IsString())
                return (uint)val.GetStrValue()!.Length;

            if (val.IsTable())
                return (uint)val.GetTableValue()!.Len();

            if (val.IsFullUserData())
                return (uint)val.GetUserDataValue()!.Size;

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
            var v = GetValueByRelIdx(idx);
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

        public bool IsLightUserData(int idx)
        {
            return Type(idx) == ELuaType.LightUserData;
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
            ToNumberX(idx, out var isNum);
            return isNum;
        }

        public bool IsInteger(int idx)
        {
            var v = GetValueByRelIdx(idx);
            return v!.IsInt();
        }

        public bool IsCSFunction(int idx)
        {
            var val = GetValueByRelIdx(idx);
            return val!.IsLCSFunction() || val!.IsCSClosure();
        }

        public bool IsUserdata(int idx)
        {
            var v = GetValueByRelIdx(idx);
            return Type(idx).IsUserdata();
        }

        public bool ToBoolean(int idx)
        {
            var val = GetValueByRelIdx(idx)!;
            return val.ToBoolean();
        }

        public LuaInt ToInteger(int idx)
        {
            return ToIntegerX(idx, out _);
        }

        public LuaInt ToIntegerX(int idx, out bool isInt)
        {
            var val = GetValueByRelIdx(idx)!;
            isInt = val.IsInt();
            return isInt ? val.GetIntValue() : (LuaInt)0;
        }

        public LuaFloat ToNumber(int idx)
        {
            var ret = ToNumberX(idx, out _);
            return ret;
        }

        public LuaFloat ToNumberX(int idx, out bool isNum)
        {
            var val = GetValueByRelIdx(idx)!;
            if (val.IsFloat())
            {
                isNum = true;
                return val.GetFloatValue();
            }

            if (val.IsInt())
            {
                isNum = true;
                return val.GetIntValue();
            }

            isNum = false;
            return 0;
        }

        public string? ToString(int idx)
        {
            return ToStringX(idx, out _);
        }

        public LuaCSFunction? ToCSFunction(int idx)
        {
            var val = GetValueByRelIdx(idx)!;
            if (val.IsLCSFunction())
                return val.GetLCSFunctionValue();
            else if (val.IsCSClosure())
                return val.GetCSClosureFunctionValue();
            return null;
        }

        public object? ToUserdata(int idx)
        {
            var v = GetValueByRelIdx(idx)!;
            switch (v.Type.GetNoVariantsType())
            {
                case ELuaType.UserData:
                    return v.GetUserDataValue()!.Memory;

                case ELuaType.LightUserData:
                    return v.GetObjValue();

                default:
                    return null;
            }
        }

        public ILuaState? ToThread(int idx)
        {
            var val = GetValueByRelIdx(idx)!;
            return val.IsThread() ? val.GetObjValue() as ILuaState : null;
        }

        public ref object? ToPointer(int idx)
        {
            // TODO
            throw new NotImplementedException();
        }

        private string? ToStringX(int idx, out bool isStr)
        {
            var val = GetValueByRelIdx(idx, out var absIdx)!;
            if (val.IsString())
            {
                isStr = true;
                return val.GetStrValue();
            }

            if (val.CanConvertToStr())
            {
                var ret = val.ToString();
                Stack[absIdx] = new LuaValue(ret, ELuaType.String);
                isStr = true;
                return ret;
            }

            isStr = false;
            return null;
        }
    }
}