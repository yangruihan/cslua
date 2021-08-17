using CsLua.API;

namespace CsLua.State
{
    internal partial class LuaState : ILuaState
    {
        public bool GC()
        {
            System.GC.Collect();
            return true;
        }
    }
}