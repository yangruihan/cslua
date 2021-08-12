using CsLua.API;

namespace CsLua.VM
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    internal static partial class InstructionAction
    {
        /// <summary>
        /// R(A) = UpValue[B] 
        /// </summary>
        public static void GetUpval(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out _);
            a += 1;
            b += 1;
            vm.Copy(vm.LuaUpvalueIndex(b), a);
        }

        /// <summary>
        /// UpValue[B] = R(A)
        /// </summary>
        public static void SetUpval(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out _);
            a += 1;
            b += 1;
            vm.Copy(a, vm.LuaUpvalueIndex(b));
        }

        /// <summary>
        /// R(A) = UpValue[B][RK(C)]
        /// </summary>
        public static void GetTabUp(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out var c);
            a += 1;
            b += 1;
            vm.GetRK(c);
            vm.GetTable(vm.LuaUpvalueIndex(b));
            vm.Replace(a);
        }

        /// <summary>
        /// UpValue[A][RK(B)] = RK(C)
        /// </summary>
        public static void SetTabUp(Instruction ins, ILuaVM vm)
        {
            ins.ABC(out var a, out var b, out var c);
            a += 1;
            vm.GetRK(b);
            vm.GetRK(c);
            vm.SetTable(vm.LuaUpvalueIndex(a));
        }
    }
}