using CsLua.API;

namespace CsLua.VM
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;
    
    partial class InstructionAction
    {
        public static void GetTabUp(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out _, out var c);
            a += 1;
            vm.PushGlobalTable();
            vm.GetRK(c);
            vm.GetTable(-2);
            vm.Replace(a);
            vm.Pop(1);
        }
    }
}