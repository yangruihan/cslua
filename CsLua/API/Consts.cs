namespace CsLua.API
{
    public enum ELuaType
    {
        None = -1,
        Nil,
        Boolean,
        LightUserData,
        Number,
        String,
        Table,
        Function,
        UserData,
        Thread,
    }

    public enum EArithOp : byte
    {
        Add,
        Sub,
        Mul,
        Mod,
        Pow,
        Div,
        IDiv,
        BAnd,
        BOr,
        BXor,
        Shl,
        Shr,
        Unm,
        BNot,
    }

    public enum ECompOp : byte
    {
        Eq,
        Lt,
        Le,
    }
}