using CsLua.API;

namespace CsLua.State
{
    internal partial class LuaState
    {
        private void PreInitThread(GlobalState g)
        {
            GlobalState = g;
            CallInfo = null;
            NCi = 0;
            NNy = 1;
            NCcalls = 0;
            Status = EStatus.Ok;
            ErrFunc = 0;
        }

        private CallInfo ExtendCI()
        {
            Assert(CallInfo.Next == null);
            CallInfo ci = new CallInfo(CallInfo);
            NCi++;
            return ci;
        }
    }
}