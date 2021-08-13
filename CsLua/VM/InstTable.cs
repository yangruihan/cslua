using CsLua.API;

namespace CsLua.VM
{
    internal static partial class InstructionAction
    {
        private const int LFIELDS_PER_FLUSH = 50;

        /// <summary>
        /// R(A) = {} (size = B, C)
        /// </summary>
        public static void NewTable(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out var c);
            a += 1;

            vm.CreateTable(Fpb.Fb2Int(b), Fpb.Fb2Int(c));
            vm.Replace(a);
        }

        /// <summary>
        /// R(A) = R(B)[RK(C)]
        /// </summary>
        public static void GetTable(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out var c);
            a += 1;
            b += 1;

            vm.GetRK(c);
            vm.GetTable(b);
            vm.Replace(a);
        }

        /// <summary>
        /// R(A)[RK(B)] = RK(C) 
        /// </summary>
        public static void SetTable(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out var c);
            a += 1;

            vm.GetRK(b);
            vm.GetRK(c);
            vm.SetTable(a);
        }

        /// <summary>
        /// R(A)[(C - 1) * FPF + i] := R(A + i), 1 <= i <= B 
        /// </summary>
        public static void SetList(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out var c);
            a += 1;

            if (c > 0)
                c--;
            else
                new Instruction(vm.Fetch()).Ax(out c);

            var bIsZero = b == 0;
            if (bIsZero)
            {
                b = (int) vm.ToInteger(-1) - a - 1;
                vm.Pop(1);
            }

            vm.CheckStack(1);
            var idx = (LuaInt) (c * LFIELDS_PER_FLUSH);
            for (var j = 1; j <= b; j++)
            {
                idx++;
                vm.PushValue(a + j);
                vm.SetI(a, idx);
            }

            if (bIsZero)
            {
                for (var j = vm.RegisterCount() + 1; j <= vm.GetTop(); j++)
                {
                    idx++;
                    vm.PushValue(j);
                    vm.SetI(a, idx);
                }

                vm.SetTop(vm.RegisterCount());
            }
        }
    }
}