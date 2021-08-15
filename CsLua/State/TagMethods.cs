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
        private LuaValue? GetTmByObj(LuaValue o, ETagMethods @event)
        {
            LuaTable mt;
            switch (o.Type.GetNoVariantsType())
            {
                case ELuaType.Table:
                    mt = o.GetTableValue().MetaTable;
                    break;

                case ELuaType.UserData:
                    mt = o.GetUserDataValue().MetaTable;
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

        private void CallTM(LuaValue f, LuaValue p1, LuaValue p2, LuaValue p3, bool hasRes)
        {
            int func = Top;
            Push()
        }

        private void CallTM(int f, int p1, int p2, int p3, bool hasRes)
        {
            var result = SaveStack(p3);
            var func = Top;
            var funcV = Index2Addr(f)!;
            var p1v = Index2Addr(p1)!;
            var p2v = Index2Addr(p2)!;

            Stack.Push(funcV); // push function (assume EXTRA_STACK)
            Stack.Push(p1v); // 1st argument
            Stack.Push(p2v); // 2nd argument

            if (!hasRes) // no result? 'p3' is third argument
            {
                var p3v = Index2Addr(p3)!;
                Stack.Push(p3v);
            }

            if (CallInfo.IsLua())
            {

            }
            else
            {
            }
        }
    }
}