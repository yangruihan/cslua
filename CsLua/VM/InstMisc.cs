using CsLua.API;
using CsLua.Common;

namespace CsLua.VM
{
    partial class InstructionAction
    {
        /// <summary>
        /// R(A) = R(B) 
        /// </summary>
        public static void Move(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out _);
            a += 1;
            b += 1;
            vm.Copy(b, a);
        }

        /// <summary>
        /// pc += sBx
        /// if (A) close all upvalues >= R(A - 1)
        /// </summary>
        public static void Jump(Instruction ins, ILuaVM vm)
        {
            ins.AsBx(out var a, out var sbx);
            vm.AddPC(sbx);

            if (a != 0)
                vm.CloseUpvalues(a);
        }
    }
}