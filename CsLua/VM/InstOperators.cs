using CsLua.API;

namespace CsLua.VM
{
    partial class InstructionAction
    {
        public static LuaAction Add = (ins, vm) => BinaryArith(ins, vm, EArithOp.Add);
        public static LuaAction Sub = (ins, vm) => BinaryArith(ins, vm, EArithOp.Sub);
        public static LuaAction Mul = (ins, vm) => BinaryArith(ins, vm, EArithOp.Mul);
        public static LuaAction Mod = (ins, vm) => BinaryArith(ins, vm, EArithOp.Mod);
        public static LuaAction Pow = (ins, vm) => BinaryArith(ins, vm, EArithOp.Pow);
        public static LuaAction Div = (ins, vm) => BinaryArith(ins, vm, EArithOp.Div);
        public static LuaAction IDiv = (ins, vm) => BinaryArith(ins, vm, EArithOp.IDiv);
        public static LuaAction BAnd = (ins, vm) => BinaryArith(ins, vm, EArithOp.BAnd);
        public static LuaAction BOr = (ins, vm) => BinaryArith(ins, vm, EArithOp.BOr);
        public static LuaAction BXor = (ins, vm) => BinaryArith(ins, vm, EArithOp.BXor);
        public static LuaAction Shl = (ins, vm) => BinaryArith(ins, vm, EArithOp.Shl);
        public static LuaAction Shr = (ins, vm) => BinaryArith(ins, vm, EArithOp.Shr);
        public static LuaAction Unm = (ins, vm) => UnaryArith(ins, vm, EArithOp.Unm);
        public static LuaAction BNot = (ins, vm) => UnaryArith(ins, vm, EArithOp.BNot);

        public static LuaAction Eq = (ins, vm) => Compare(ins, vm, ECompOp.Eq);
        public static LuaAction Lt = (ins, vm) => Compare(ins, vm, ECompOp.Lt);
        public static LuaAction Le = (ins, vm) => Compare(ins, vm, ECompOp.Le);

        /// <summary>
        /// R(A) = not R(B)
        /// </summary>
        public static LuaAction Not = (ins, vm) =>
        {
            ins.ABC(out var a, out var b, out _);
            a += 1;
            b += 1;
            vm.PushBoolean(!vm.ToBoolean(b));
            vm.Replace(a);
        };

        /// <summary>
        /// if not (R(A) != C)
        ///     pc++
        /// </summary>
        public static LuaAction Test = (ins, vm) =>
        {
            ins.ABC(out var a, out _, out var c);
            a += 1;

            if (vm.ToBoolean(a) != (c != 0))
                vm.AddPC(1);
        };

        /// <summary>
        /// if (R(B) != C)
        ///     R(A) = R(B)
        /// else
        ///     pc++
        /// </summary>
        public static LuaAction TestSet = (ins, vm) =>
        {
            ins.ABC(out var a, out var b, out var c);
            a += 1;
            b += 1;
            if (vm.ToBoolean(b) == (c != 0))
                vm.Copy(b, a);
            else
                vm.AddPC(1);
        };

        /// <summary>
        /// R(A) = length of R(B)
        /// </summary>
        public static LuaAction Length = (ins, vm) =>
        {
            ins.ABC(out var a, out var b, out _);
            a += 1;
            b += 1;
            vm.Len(b);
            vm.Replace(a);
        };

        /// <summary>
        /// R(A) = R(B) .. ... .. R(C)
        /// </summary>
        public static LuaAction Concat = (ins, vm) =>
        {
            ins.ABC(out var a, out var b, out var c);
            a += 1;
            b += 1;
            c += 1;

            var n = c - b + 1;
            vm.CheckStack(n);
            for (var i = b; i <= c; i++)
                vm.PushValue(i);
            vm.Concat(n);
            vm.Replace(a);
        };

        /// <summary>
        /// R(A) = RK(B) op RK(C) 
        /// </summary>
        private static void BinaryArith(Instruction ins, ILuaVM vm, EArithOp op)
        {
            ins.ABC(out var a, out var b, out var c);
            a += 1;

            vm.GetRK(b);
            vm.GetRK(c);
            vm.Arith(op);
            vm.Replace(a);
        }

        /// <summary>
        /// R(A) = op R(B) 
        /// </summary>
        private static void UnaryArith(Instruction ins, ILuaVM vm, EArithOp op)
        {
            ins.ABC(out var a, out var b, out _);
            a += 1;
            b += 1;

            vm.PushValue(b);
            vm.Arith(op);
            vm.Replace(a);
        }

        /// <summary>
        /// if ((RK(B) op RK(C)) != A)
        ///     pc++ 
        /// </summary>
        private static void Compare(Instruction ins, ILuaVM vm, ECompOp op)
        {
            ins.ABC(out var a, out var b, out var c);

            vm.GetRK(b);
            vm.GetRK(c);

            if (vm.Compare(-2, -1, op) != (a != 0))
                vm.AddPC(1);

            vm.Pop(2);
        }
    }
}