using CsLua.API;

namespace CsLua.State
{
    internal enum ETagMethods
    {
        INDEX,
        NEWINDEX,
        GC,
        MODE,
        LEN,
        EQ,
        ADD,
        SUB,
        MUL,
        MOD,
        POW,
        DIV,
        IDIV,
        BAND,
        BOR,
        BXOR,
        SHL,
        SHR,
        UNM,
        BNOT,
        LT,
        LE,
        CONCAT,
        CALL,
        N // number of elements in the enum
    }

    internal static class TagMethods
    {
        public static readonly string[] TagMethodName =
        {
            "__index", "__newindex",
            "__gc", "__mode", "__len", "__eq",
            "__add", "__sub", "__mul", "__mod", "__pow",
            "__div", "__idiv",
            "__band", "__bor", "__bxor", "__shl", "__shr",
            "__unm", "__bnot", "__lt", "__le",
            "__concat", "__call"
        };

        public static string Get(ETagMethods tagMethod)
        {
            return TagMethodName[(int)tagMethod];
        }
    }

    internal partial class LuaState
    {
        private LuaValue? GetTMByObj(LuaValue o, ETagMethods @event)
        {
            LuaTable mt;
            switch (o.Type.GetNoVariantsType())
            {
                case ELuaType.Table:
                    mt = o.GetTableValue()!.MetaTable;
                    break;

                case ELuaType.UserData:
                    mt = o.GetUserDataValue()!.MetaTable;
                    break;

                default:
                    mt = GlobalState.Mt[(int)o.Type.GetNoVariantsType()];
                    break;
            }

            return mt != null ? mt.Get(GlobalState.TmNames[(int)@event]) : null;
        }

        private string ObjTypeName(LuaValue o)
        {
            LuaTable mt;
            if ((o.IsTable() && (mt = o.GetTableValue()!.MetaTable) != null)
            || (o.IsFullUserData() && (mt = o.GetUserDataValue()!.MetaTable) != null))
            {
                var name = mt.Get("__name");
                if (name.IsString())
                    return name.GetStrValue()!;
            }

            return TypeName(o.Type.GetNoVariantsType());
        }

        private void CallTM(LuaValue funcV, LuaValue p1v, LuaValue p2v, int p3, bool hasRes)
        {
            var result = SaveStack(p3);
            var func = Top;
            Stack.Push(funcV); // push function (assume EXTRA_STACK)
            Stack.Push(p1v); // 1st argument
            Stack.Push(p2v); // 2nd argument

            if (!hasRes) // no result? 'p3' is third argument
            {
                // 3rd argument
                var p3v = Index2Addr(p3)!;
                Stack.Push(p3v);
            }

            // metamethod may yield only when called from Lua code
            if (CallInfo.IsLua())
                InnerCall(func, hasRes ? 1 : 0);
            else
                CallNoYield(func, hasRes ? 1 : 0);

            if (hasRes) // if has result, move it to its place
            {
                p3 = RestoreStack(result);
                SetValue(p3, Pop());
            }
        }

        private void CallTM(int f, int p1, int p2, int p3, bool hasRes)
        {
            var funcV = Index2Addr(f)!;
            var p1v = Index2Addr(p1)!;
            var p2v = Index2Addr(p2)!;

            CallTM(funcV, p1v, p2v, p3, hasRes);
        }

        private bool CallBinTM(LuaValue p1, LuaValue p2, int res, ETagMethods @event)
        {
            var tm = GetTMByObj(p1, @event); // try first operand
            if (tm == null || tm.IsNil())
                tm = GetTMByObj(p2, @event); // try second operand
            if (tm == null || tm.IsNil())
                return false;
            CallTM(tm, p1, p2, res, true);
            return true;
        }

        private bool CallBinTM(int p1, int p2, int res, ETagMethods @event)
        {
            return CallBinTM(Index2Addr(p1)!, Index2Addr(p2)!, res, @event);
        }

        private void TryBinTM(int p1, int p2, int res, ETagMethods @event)
        {
            if (!CallBinTM(p1, p2, res, @event))
            {
                switch (@event)
                {
                    case ETagMethods.CONCAT:
                        {
                            ConcatError(p1, p2);
                            break;
                        }

                    case ETagMethods.BAND:
                    case ETagMethods.BOR:
                    case ETagMethods.BXOR:
                    case ETagMethods.SHL:
                    case ETagMethods.SHR:
                    case ETagMethods.BNOT:
                        {
                            if (InnerToNumber(p1, out var dummoy)
                                && InnerToNumber(p2, out dummoy))
                                ToIntError(p1, p2);
                            else
                                OpIntError(p1, p2, "perform bitwise operation on");
                            break;
                        }

                    default:
                        {
                            OpIntError(p1, p2, "perform arithmetic on");
                            break;
                        }
                }
            }
        }

        private int CallOrderTM(LuaValue p1, LuaValue p2, ETagMethods @event)
        {
            if (!CallBinTM(p1, p2, Top, @event))
                return -1;
            else
                return Index2Addr(Top)!.ToBoolean() ? 1 : 0;
        }
    }
}