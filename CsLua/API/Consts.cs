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
}