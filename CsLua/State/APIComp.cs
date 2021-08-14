using System;
using CsLua.API;
using CsLua.Misc;

namespace CsLua.State
{
    /// <summary>
    /// 比较运算实现
    /// </summary>
    internal partial class LuaState : ILuaState
    {
        public bool RawEqual(int idx1, int idx2)
        {
            var v1 = Index2Addr(idx1);
            var v2 = Index2Addr(idx2);

            return (LuaAPI.IsValid(v1) && LuaAPI.IsValid(v2))
                    ? Eq(v1!, v2!, null) 
                    : false;
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