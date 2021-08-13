namespace CsLua.Misc
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
    }
}