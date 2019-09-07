using CsLua.API;
using CsLua.Common;

namespace CsLua.State
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    partial class LuaState : ILuaState
    {
        public bool RawEqual(int idx1, int idx2)
        {
            if (_stack.IsValid(idx1) || _stack.IsValid(idx2))
            {
                return false;
            }

            var a = _stack[idx1];
            var b = _stack[idx2];
            return Eq(a, b, null);
        }

        public bool Compare(int idx1, int idx2, ECompOp op)
        {
            if (!_stack.IsValid(idx1) || !_stack.IsValid(idx2))
                return false;

            var a = _stack[idx1];
            var b = _stack[idx2];

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
            if (a.Value is null)
            {
                return b.Value is null;
            }
            else if (a.Value is bool aB)
            {
                return b.Value is bool bB && aB == bB;
            }
            else if (a.Value is string aS)
            {
                return b.Value is string bS && aS == bS;
            }
            else if (a.Value is LuaInt aI)
            {
                if (b.Value is LuaInt bI)
                    return aI == bI;
                else if (b.Value is LuaFloat bF)
                    return aI == bF;
                else
                    return false;
            }
            else if (a.Value is LuaFloat aF)
            {
                if (b.Value is LuaFloat bF)
                    return aF == bF;
                else if (b.Value is LuaInt bI)
                    return aF == bI;
                else
                    return false;
            }
            else if (a.Value is LuaTable al
                     && b.Value is LuaTable bl
                     && al != bl && ls != null)
            {
                if (LuaValue.CallMetaMethod(a, b, "__eq", ls, out var metaMethodRet))
                {
                    return metaMethodRet.ToBoolean();
                }

                return a.Value == b.Value;
            }
            else
            {
                return a.Value == b.Value;
            }
        }

        private bool Lt(LuaValue a, LuaValue b, LuaState ls)
        {
            if (a.Value is string aS)
            {
                return b.Value is string bS && aS.CompareTo(bS) < 0;
            }
            else if (a.Value is LuaInt aI)
            {
                if (b.Value is LuaInt bI)
                    return aI < bI;
                else if (b.Value is LuaFloat bF)
                    return aI < bF;
            }
            else if (a.Value is LuaFloat aF)
            {
                if (b.Value is LuaFloat bF)
                    return aF < bF;
                else if (b.Value is LuaInt bI)
                    return aF < bI;
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
            if (a.Value is string aS)
            {
                return b.Value is string bS && aS.CompareTo(bS) <= 0;
            }
            else if (a.Value is LuaInt aI)
            {
                if (b.Value is LuaInt bI)
                    return aI <= bI;
                else if (b.Value is LuaFloat bF)
                    return aI <= bF;
            }
            else if (a.Value is LuaFloat aF)
            {
                if (b.Value is LuaFloat bF)
                    return aF <= bF;
                else if (b.Value is LuaInt bI)
                    return aF <= bI;
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