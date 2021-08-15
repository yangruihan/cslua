using CsLua.API;

namespace CsLua.State
{
    internal partial class LuaState
    {
        private static class _VM
        {
            public static bool LTIntFloat(LuaInt i, LuaFloat f)
            {
                return i < f;
            }

            public static bool LEIntFloat(LuaInt i, LuaFloat f)
            {
                return i <= f;
            }

            public static bool LTNum(LuaValue l, LuaValue r)
            {
                if (l.IsInt())
                {
                    var li = l.GetIntValue();
                    if (r.IsInt()) // both are integers
                        return li < r.GetIntValue();
                    else
                        return LTIntFloat(li, r.GetFloatValue());
                }
                else
                {
                    var lf = l.GetFloatValue();
                    if (r.IsFloat()) // /* both are float */
                        return lf < r.GetFloatValue();
                    else if (LuaFloat.IsNaN(lf)) // 'r' is int and 'l' is float
                        return false; // NaN < i is always false
                    else
                        return !LEIntFloat(r.GetIntValue(), lf);
                }
            }

            public static bool LENum(LuaValue l, LuaValue r)
            {
                if (l.IsInt())
                {
                    var li = l.GetIntValue();
                    if (r.IsInt()) // both are integers
                        return li <= r.GetIntValue();
                    else // 'l' is int and 'r' is float
                        return LEIntFloat(li, r.GetFloatValue()); // l <= r ?
                }
                else
                {
                    var lf = l.GetFloatValue(); // 'l' must be float
                    if (r.IsFloat())
                        return lf <= r.GetFloatValue(); // both are float
                    else if (LuaFloat.IsNaN(lf)) // 'r' is int and 'l' is float
                        return false; //  NaN <= i is always false
                    else
                        return !LTIntFloat(r.GetIntValue(), lf); // not (r < l) ?
                }
            }
        }

        private bool InnerToNumber(int idx, out LuaFloat num)
        {
            num = ToNumberX(idx, out var isNum);
            return isNum;
        }

        private bool InnerToInteger(int idx, out LuaInt num)
        {
            num = ToIntegerX(idx, out var isNum);
            return isNum;
        }

        private bool EqualObj(LuaValue a, LuaValue b)
        {
            if (a.Type != b.Type) // not the same variant?
            {
                // only numbers can be equal with different variants
                if (a.Type.GetNoVariantsType() != b.Type.GetNoVariantsType() || a.Type.GetNoVariantsType() != ELuaType.Number)
                    return false;
                else
                {
                    return (a.ToInteger(out var a1) && b.ToInteger(out var b1) && a1 == b1);
                }
            }

            LuaValue? tm = null;

            switch (a.Type)
            {
                case ELuaType.Nil:
                    return true;

                case ELuaType.Int:
                    return a.GetIntValue() == b.GetIntValue();

                case ELuaType.Float:
                    return a.GetFloatValue() == b.GetFloatValue();

                case ELuaType.Boolean:
                    return a.GetBoolValue() == b.GetBoolValue();

                case ELuaType.LightUserData:
                    return a.GetObjValue() == b.GetObjValue();

                case ELuaType.LCSFunction:
                    return a.GetLCSFunctionValue() == b.GetLCSFunctionValue();

                case ELuaType.String:
                    return a.GetStrValue() == b.GetStrValue();

                case ELuaType.UserData:
                    {
                        var u1 = a.GetUserDataValue()!;
                        var u2 = b.GetUserDataValue()!;
                        if (u1 == u2)
                            return true;

                        // will try tm
                        if (u1.MetaTable != null)
                        {
                            tm = u1.MetaTable.Get(TagMethods.Get(ETagMethods.EQ));
                        }

                        if (tm == null && u2.MetaTable != null)
                        {
                            tm = u2.MetaTable.Get(TagMethods.Get(ETagMethods.EQ));
                        }

                        break;
                    }

                case ELuaType.Table:
                    {
                        var u1 = a.GetTableValue()!;
                        var u2 = a.GetTableValue()!;
                        if (u1 == u2)
                            return true;

                        // will try tm
                        if (u1.MetaTable != null)
                        {
                            tm = u1.MetaTable.Get(TagMethods.Get(ETagMethods.EQ));
                        }

                        if (tm == null && u2.MetaTable != null)
                        {
                            tm = u2.MetaTable.Get(TagMethods.Get(ETagMethods.EQ));
                        }

                        break;
                    }

                default:
                    return a.GetObjValue() == b.GetObjValue();
            }

            // no TM?, objects are different
            if (tm == null)
                return false;

            CallTM(tm, a, b, Top, true);
            return Index2Addr(Top)!.ToBoolean();
        }

        private bool EqualObj(int indexA, int indexB)
        {
            var a = Index2Addr(indexA)!;
            var b = Index2Addr(indexB)!;
            return EqualObj(a, b);
        }

        private bool LessThan(LuaValue l, LuaValue r)
        {
            int ret;
            // both operands are numbers?
            if (l.IsNumber() && r.IsNumber())
                return _VM.LTNum(l, r);
            else if (l.IsString() && r.IsString()) // both are strings
                return string.Compare(l.GetStrValue(), r.GetStrValue()) < 0;
            else if ((ret = CallOrderTM(l, r, ETagMethods.LT)) < 0)
                OrderError(l, r);
            return ret == 1 ? true : false;
        }

        private bool LessEqual(LuaValue l, LuaValue r)
        {
            int ret;
            // both operands are numbers?
            if (l.IsNumber() && r.IsNumber())
                return _VM.LENum(l, r);
            else if (l.IsString() && r.IsString()) // both are strings
                return string.Compare(l.GetStrValue(), r.GetStrValue()) <= 0;
            else if ((ret = CallOrderTM(l, r, ETagMethods.LE)) >= 0) // try 'le'
                return ret == 1 ? true : false;
            else // try 'lt':
            {
                CallInfo.CallStatus |= CallInfoStatus.LEQ; // mark it is doing 'lt' for 'le'
                ret = CallOrderTM(r, l, ETagMethods.LT);
                CallInfo.CallStatus ^= CallInfoStatus.LEQ; // clear mark
                if (ret < 0)
                    OrderError(l, r);
                return !(ret == 0 ? false : true); // result is negated
            }
        }
    }
}