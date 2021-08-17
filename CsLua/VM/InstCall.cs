using CsLua.API;

namespace CsLua.VM
{
    internal static partial class InstructionAction
    {
        /// <summary>
        /// R(A + 1) = R(B)
        /// R(A) = R(B)[RK(C)] 
        /// </summary>
        public static void Self(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out var c);
            a += 1;
            b += 1;

            vm.Copy(b, a + 1);
            vm.GetRK(c);
            vm.GetTable(b);
            vm.Replace(a);
        }

        /// <summary>
        /// R(A) = closure(KPROTO[Bx]) 
        /// </summary>
        public static void Closure(Instruction ins, ILuaVM vm)
        {
            ins.ABx(out var a, out var bx);
            a += 1;

            vm.LoadProto(bx);
            vm.Replace(a);
        }

        /// <summary>
        /// R(A), R(A + 1), ..., R(A + B - 2) = vararg 
        /// </summary>
        public static void Vararg(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out _);
            a += 1;

            if (b != 1)
            {
                vm.LoadVararg(b - 1);
                PopResults(a, b, vm);
            }
        }

        /// <summary>
        /// return R(A)(R(A + 1), ..., R(A + B - 1)) 
        /// </summary>
        public static void TailCall(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out _);
            a += 1;

            var c = 0;
            var nArgs = PushFuncAndArgs(a, b, vm);
            vm.Call(nArgs, c - 1);
            PopResults(a, c, vm);
        }

        /// <summary>
        /// R(A), ..., R(A + C - 2) = R(A)(R(A + 1), ..., R(A + B - 1))
        /// </summary>
        public static void Call(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out var c);
            a += 1;

            var nArgs = PushFuncAndArgs(a, b, vm);
            vm.Call(nArgs, c - 1);
            PopResults(a, c, vm);
        }

        /// <summary>
        /// return R(A), ..., R(A + B - 2)
        /// </summary>
        public static void Return(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out _);
            a += 1;

            vm.FinishCall(a, b - 1);
        }

        /// <summary>
        /// R(A + 3, ..., R(A + 2 + C) = R(A)(R(A + 1), R(A + 2))
        /// </summary>
        public static void TForCall(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out _, out var c);
            a += 1;
            PushFuncAndArgs(a, 3, vm);
            vm.Call(2, c);
            PopResults(a + 3, c + 1, vm);
        }

        private static int PushFuncAndArgs(int a, int b, ILuaVM vm)
        {
            if (b >= 1)
            {
                vm.CheckStack(b);
                for (var i = a; i < a + b; i++)
                    vm.PushValue(i);

                return b - 1;
            }
            else
            {
                FixStack(a, vm);
                return vm.GetTop() - vm.RegisterCount() - 1;
            }
        }

        private static void FixStack(int a, ILuaVM vm)
        {
            var x = (int)vm.ToInteger(-1);
            vm.Pop(1);

            vm.CheckStack(x - a);
            for (var i = a; i < x; i++)
                vm.PushValue(i);
            vm.Rotate(vm.RegisterCount() + 1, x - a);
        }

        private static void PopResults(int a, int c, ILuaVM vm)
        {
            if (c == 1)
            {
            }
            else if (c > 1)
            {
                for (var i = a + c - 2; i >= a; i--)
                    vm.Replace(i);
            }
            else
            {
                vm.CheckStack(1);
                vm.PushInteger(a);
            }
        }
    }
}