namespace CsLua.State
{
    internal partial class LuaState
    {
        private void SetValue(int idx, LuaValue v)
        {
            idx = CallInfo.Func + idx;
            Stack[idx] = v;
        }
    }
}