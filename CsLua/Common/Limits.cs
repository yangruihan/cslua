using CsLua.API;

namespace CsLua.Common
{
    internal static class Limits
    {
        public static void Check(ILuaState l, bool e, string msg)
        {
            System.Diagnostics.Debug.Assert(e, msg);
        }
    }
}