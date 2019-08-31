using System;

namespace CsLua.API
{
    public interface ILuaVM : ILuaState
    {
        int PC();
        void AddPC(int n);
        UInt32 Fetch();
        void GetConst(int idx);
        void GetRK(int rk);
    }
}