using System;
using CsLua.API;
using CsLua.Common;

namespace CsLua.State
{
    /// <summary>
    /// 比较运算实现
    /// </summary>
    internal partial class LuaState : ILuaState
    {
        public bool RawEqual(int idx1, int idx2)
        {
            if (Stack.IsValid(idx1) || Stack.IsValid(idx2))
            {
                return false;
            }

            var a = Stack[idx1];
            var b = Stack[idx2];
            return Eq(a, b, null);
        }

        /// <summary>
        /// 对指定索引处的两个值进行比较，返回结果
        /// </summary>
        public bool Compare(int idx1, int idx2, ECompOp op)
        {
            if (!Stack.IsValid(idx1) || !Stack.IsValid(idx2))
                return false;

            var a = Stack[idx1];
            var b = Stack[idx2];

            switch (op)
            {
                case ECompOp.Eq:
                    return Eq(a, b, this);
                case ECompOp.Lt:
                    return Lt(a, b, this);
                case ECompOp.Le:
                    return Le(a, b, this);
                default:
                    Debug.Panic("invalid compare op!");
                    return false;
            }
        }

        private bool Eq(LuaValue a, LuaValue b, LuaState ls)
        {
            if (a.IsNil())
            {
                return b.IsNil();
            }
            else if (a.IsBool())
            {
                return b.IsBool() && a.GetBoolValue() == b.GetBoolValue();
            }
            else if (a.IsString())
            {
                return b.IsString() && a.GetStrValue() == b.GetStrValue();
            }
            else if (a.IsInt())
            {
                if (b.IsInt())
                    return a.GetIntValue() == b.GetIntValue();
                else if (b.IsFloat())
                    return a.GetIntValue() == b.GetFloatValue();
                else
                    return false;
            }
            else if (a.IsFloat())
            {
                if (b.IsFloat())
                    return a.GetFloatValue() == b.GetFloatValue();
                else if (b.IsInt())
                    return a.GetFloatValue() == b.GetIntValue();
                else
                    return false;
            }
            else if (a.IsTable()
                     && b.IsTable()
                     && a.GetTableValue() != b.GetTableValue() && ls != null)
            {
                if (LuaValue.CallMetaMethod(a, b, "__eq", ls, out var metaMethodRet))
                {
                    return metaMethodRet.ToBoolean();
                }

                return a.GetValue() == b.GetValue();
            }
            else
            {
                return a.GetValue() == b.GetValue();
            }
        }

        private bool Lt(LuaValue a, LuaValue b, LuaState ls)
        {
            if (a.IsString())
            {
                return b.IsString() && string.Compare(a.GetStrValue(), b.GetStrValue(), StringComparison.Ordinal) < 0;
            }
            else if (a.IsNumber() && b.IsNumber())
            {
                return a.GetFloatValue() < b.GetFloatValue();
            }

            if (LuaValue.CallMetaMethod(a, b, "__lt", ls, out var metaMethodRet))
            {
                return metaMethodRet.ToBoolean();
            }

            Debug.Panic("comparison error!");
            return false;
        }

        private bool Le(LuaValue a, LuaValue b, LuaState ls)
        {
            if (a.IsString())
            {
                return b.IsString() && string.Compare(a.GetStrValue(), b.GetStrValue(), StringComparison.Ordinal) <= 0;
            }
            else if (a.IsNumber() && b.IsNumber())
            {
                return a.GetFloatValue() <= b.GetFloatValue();
            }

            if (LuaValue.CallMetaMethod(a, b, "__le", ls, out var metaMethodRet))
            {
                return metaMethodRet.ToBoolean();
            }
            else if (LuaValue.CallMetaMethod(b, a, "__lt", ls, out metaMethodRet))
            {
                return !metaMethodRet.ToBoolean();
            }

            Debug.Panic("comparison error!");
            return false;
        }
    }
}