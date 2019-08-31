using CsLua.API;

namespace CsLua.VM
{
    partial class InstructionAction
    {
        /// <summary>
        /// R(A), R(A + 1), ..., R(A + B) = nil 
        /// </summary>
        public static void LoadNil(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out _);
            a += 1;

            vm.PushNil();
            for (var j = a; j <= a + b; j++)
                vm.Copy(-1, j);
            vm.Pop(1);
        }

        /// <summary>
        /// R(A) = (bool) B
        /// if (C) pc++ 
        /// </summary>
        public static void LoadBool(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out var c);
            a += 1;

            vm.PushBoolean(b != 0);
            vm.Replace(a);

            if (c != 0)
                vm.AddPC(1);
        }

        /// <summary>
        /// R(A) = Kst(Bx) 
        /// </summary>
        public static void LoadK(Instruction ins, ILuaVM vm)
        {
            ins.ABx(out var a, out var bx);
            a += 1;

            vm.GetConst(bx);
            vm.Replace(a);
        }

        /// <summary>
        /// R(A) = Kst(extra arg)
        /// </summary>
        public static void LoadKx(Instruction ins, ILuaVM vm)
        {
            ins.ABx(out var a, out _);
            a += 1;
            new Instruction(vm.Fetch()).Ax(out var ax);
            vm.GetConst(ax);
            vm.Replace(a);
        }
    }
}