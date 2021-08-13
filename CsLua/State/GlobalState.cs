using System.Collections.Generic;
using CsLua.API;
using CsLua.Misc;

namespace CsLua.State
{
    /// <summary>
    /// 全局状态，所有线程共享
    /// </summary>
    internal class GlobalState
    {
        public LuaValue Registry { get; private set; }
        public LuaTable RegistryTable => Registry.GetTableValue();

        /// <summary>
        /// list of threads with open upvalues
        /// Threads with upvalues
        /// </summary>
        public List<LuaState> Twups;

        /// <summary>
        /// to be called in unprotected errors
        /// </summary>
        public LuaCSFunction Panic;

        public LuaState MainThread { get; private set; }

        public LuaFloat Version { get; private set; }

        public string[] TmNames = TagMethods.TagMethodName;

        public LuaTable[] Mt = new LuaTable[(int) ELuaType.NumTags];

        public GlobalState(LuaState mainThread)
        {
            MainThread = mainThread;
        }

        public void Init(LuaState mainThread)
        {
            InitRegistry(mainThread);

            Version = CsLua.Version();
        }

        public void SetRegistry(LuaTable luaTable)
        {
            Registry = new LuaValue(luaTable, ELuaType.Table);
        }

        private void InitRegistry(LuaState mainThread)
        {
            var registryTable = new LuaTable(0, 0);
            Registry = new LuaValue(registryTable, ELuaType.Table);

            // registry[LUA_RIDX_MAINTHREAD] = L
            registryTable.Put(LuaConst.LUA_RIDX_MAINTHREAD, new LuaValue(mainThread, ELuaType.Thread));

            // registry[LUA_RIDX_GLOBALS] = table of globals
            registryTable.Put(LuaConst.LUA_RIDX_GLOBALS, new LuaTable(0, 0));
        }
    }
}