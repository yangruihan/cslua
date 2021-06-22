using CsLua.API;

namespace CsLua.State
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    partial class LuaState : ILuaState
    {
        public int Error(string msg)
        {
            PushString(msg);
            return Error();
        }

        public void CheckStack(int n, string errorMsg)
        {
            if (!CheckStack(n))
                Error(errorMsg);
        }

        public LuaFloat CheckNumber(int arg)
        {
            if (!ToNumberX(arg, out var ret))
                Error($"Expected number, but {_stack[arg].Type}");

            return ret;
        }

        public void NewLib(LuaReg[] lib)
        {
            CreateTable(0, lib.Length);
            SetFuncs(lib, 0);
        }

        public void SetFuncs(LuaReg[] lib, int nup)
        {
            CheckStack(nup, "too many upvalues");

            // fill the table with given functions
            for (var i = 0; i < lib.Length; i++)
            {
                // copy upvalues to the top
                for (var j = 0; j < nup; j++)
                    PushValue(-nup);

                // closure with those upvalues
                PushCSClosure(lib[i].Func, nup);
                SetField(-(nup + 2), lib[i].Name);
            }

            Pop(nup); // remove upvalues
        }
    }
}