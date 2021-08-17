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
            var v1 = GetValueByRelIdx(idx1)!;
            var v2 = GetValueByRelIdx(idx2)!;

            return (LuaAPI.IsValid(v1) && LuaAPI.IsValid(v2))
                    ? EqualObj(v1, v2)
                    : false;
        }

        /// <summary>
        /// 对指定索引处的两个值进行比较，返回结果
        /// </summary>
        public bool Compare(int idx1, int idx2, ECompOp op)
        {
            var a = GetValueByRelIdx(idx1)!;
            var b = GetValueByRelIdx(idx2)!;

            if (LuaAPI.IsValid(a) && LuaAPI.IsValid(b))
            {
                switch (op)
                {
                    case ECompOp.Eq:
                        return EqualObj(a, b);
                    case ECompOp.Lt:
                        return LessThan(a, b);
                    case ECompOp.Le:
                        return LessEqual(a, b);
                    default:
                        Check(false, "invalid option");
                        return false;
                }
            }

            return false;
        }
    }
}