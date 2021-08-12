using System;
using System.Collections.Generic;
using CsLua.API;

namespace CsLua.State
{
    using LuaInt = Int64;
    using LuaFloat = Double;

    /// <summary>
    /// 全局状态，所有线程共享
    /// </summary>
    internal class GlobalState
    {
        public LuaValue Registry;

        /// <summary>
        /// list of threads with open upvalues
        /// Threads with upvalues
        /// </summary>
        public List<LuaState> Twups;

        /// <summary>
        /// to be called in unprotected errors
        /// </summary>
        public LuaCSFunction Panic;

        public LuaState MainThread;

        public LuaFloat Version;

        public LuaTable[] Mt = new LuaTable[(int) ELuaType.NumTags];

        public GlobalState(LuaState mainThread)
        {
            MainThread = mainThread;
        }
    }
}