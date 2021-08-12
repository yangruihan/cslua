using System;
using CsLua.API;
using CsLua.State;

namespace CsLua
{
    using LuaInt = Int64;
    using LuaFloat = Double;

    public static class CsLua
    {
        public static ILuaVM CreateLuaVM()
        {
            return new LuaState();
        }

        public static ILuaState CreateLuaState()
        {
            return new LuaState();
        }

        public static ILuaState NewThread(ILuaState l)
        {
            return LuaState.NewThread(l as LuaState);
        }

        public static LuaFloat Version(ILuaState l = null)
        {
            if (l != null)
                return l.Version();

            return LuaConst.LUA_VERSION_NUM;
        }
    }
}