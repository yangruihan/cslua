using CsLua.API;
using CsLua.State;

public static class CSLua
{
    public static ILuaVM CreateLuaVM()
    {
        return new LuaState();
    }
    
    public static ILuaState CreateLuaState()
    {
        return new LuaState();
    }
}