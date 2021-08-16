using System;

namespace CsLua.API
{
    internal interface ILuaVM : ILuaState
    {
        int PC();
        void AddPC(int n);
        UInt32 Fetch();
        void GetConst(int idx);
        void GetRK(int rk);

        int RegisterCount();
        void LoadVararg(int n);
        void LoadProto(int idx);

        void FinishCall(int first, int n);
        void CloseUpvalues(int a);
    }
}