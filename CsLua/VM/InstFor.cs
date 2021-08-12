using CsLua.API;

namespace CsLua.VM
{
    internal static partial class InstructionAction
    {
        /// <summary>
        /// R(A) -= R(A + 2)
        /// pc += sBx 
        /// </summary>
        public static void ForPrep(Instruction ins, ILuaVM vm)
        {
            ins.AsBx(out var a, out var sbx);
            a += 1;

            if (vm.Type(a) == ELuaType.String)
            {
                vm.PushNumber(vm.ToNumber(a));
                vm.Replace(a);
            }

            if (vm.Type(a + 1) == ELuaType.String)
            {
                vm.PushNumber(vm.ToNumber(a + 1));
                vm.Replace(a + 1);
            }

            if (vm.Type(a + 2) == ELuaType.String)
            {
                vm.PushNumber(vm.ToNumber(a + 2));
                vm.Replace(a + 2);
            }

            // R(A) -= R(A + 2)
            vm.PushValue(a);
            vm.PushValue(a + 2);
            vm.Arith(EArithOp.Sub);
            vm.Replace(a);

            // pc += sBx 
            vm.AddPC(sbx);
        }

        /// <summary>
        /// R(A) += R(A + 2)
        /// if R(A) <?= R(A + 1) {
        ///     pc += sBx
        ///     R(A + 3) = R(A)
        /// }
        /// </summary>
        public static void ForLoop(Instruction ins, ILuaVM vm)
        {
            ins.AsBx(out var a, out var sbx);
            a += 1;

            // R(A) += R(A + 2)
            vm.PushValue(a + 2);
            vm.PushValue(a);
            vm.Arith(EArithOp.Add);
            vm.Replace(a);

            var isPositiveStep = vm.ToNumber(a + 2) >= 0;
            if (isPositiveStep && vm.Compare(a, a + 1, ECompOp.Le)
                || !isPositiveStep && vm.Compare(a + 1, a, ECompOp.Le))
            {
                // pc += sBx
                vm.AddPC(sbx);
                // R(A + 3) = R(A)
                vm.Copy(a, a + 3);
            }
        }

        /// <summary>
        /// if R(A + 1) != null
        ///     R(A) = R(A + 1); pc += sBx 
        /// </summary>
        public static void TForLoop(Instruction ins, ILuaVM vm)
        {
            ins.AsBx(out var a, out var sbx);
            a += 1;
            if (!vm.IsNil(a + 1))
            {
                vm.Copy(a + 1, a);
                vm.AddPC(sbx);
            }
        }
    }
}