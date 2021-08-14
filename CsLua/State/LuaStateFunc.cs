namespace CsLua.State
{
    internal partial class LuaState
    {
        private void Close(int level)
        {
            CloseUpvalues(level);
        }
    }
}