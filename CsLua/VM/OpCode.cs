using CsLua.API;

namespace CsLua.VM
{
    using I = InstructionAction;

    enum EOpMode : byte
    {
        IABC,
        IABx,
        IAsBx,
        IAx,
    }

    enum EOpArgMask : byte
    {
        OpArgN,
        OpArgU,
        OpArgR,
        OpArgK,
    }

    enum EOpCode : byte
    {
        OP_MOVE,
        OP_LOADK,
        OP_LOADKX,
        OP_LOADBOOL,
        OP_LOADNIL,
        OP_GETUPVAL,
        OP_GETTABUP,
        OP_GETTABLE,
        OP_SETTABUP,
        OP_SETUPVAL,
        OP_SETTABLE,
        OP_NEWTABLE,
        OP_SELF,
        OP_ADD,
        OP_SUB,
        OP_MUL,
        OP_MOD,
        OP_POW,
        OP_DIV,
        OP_IDIV,
        OP_BAND,
        OP_BOR,
        OP_BXOR,
        OP_SHL,
        OP_SHR,
        OP_UNM,
        OP_BNOT,
        OP_NOT,
        OP_LEN,
        OP_CONCAT,
        OP_JMP,
        OP_EQ,
        OP_LT,
        OP_LE,
        OP_TEST,
        OP_TESTSET,
        OP_CALL,
        OP_TAILCALL,
        OP_RETURN,
        OP_FORLOOP,
        OP_FORPREP,
        OP_TFORCALL,
        OP_TFORLOOP,
        OP_SETLIST,
        OP_CLOSURE,
        OP_VARARG,
        OP_EXTRAARG,
    }

    delegate void LuaAction(Instruction ins, ILuaVM vm);

    struct OpCode
    {
        public byte TestFlag;
        public byte SetAFlag;
        public EOpArgMask ArgBMode;
        public EOpArgMask ArgCMode;
        public EOpMode OpMode;
        public string Name;
        public LuaAction Action;

        public OpCode(byte testFlag, byte setAFlag, EOpArgMask argBMode, EOpArgMask argCMode, EOpMode opMode,
            string name, LuaAction action)
        {
            TestFlag = testFlag;
            SetAFlag = setAFlag;
            ArgBMode = argBMode;
            ArgCMode = argCMode;
            OpMode = opMode;
            Name = name;
            Action = action;
        }
    }

    static class OpCodes
    {
        public static OpCode[] Codes =
        {
            new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IABC, "MOVE    ", I.Move), // R(A) := R(B)
            new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgN, EOpMode.IABx, "LOADK   ",
                I.LoadK), // R(A) := Kst(Bx)
            new OpCode(0, 1, EOpArgMask.OpArgN, EOpArgMask.OpArgN, EOpMode.IABx, "LOADKX  ",
                I.LoadKx), // R(A) := Kst(extra arg)
            new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgU, EOpMode.IABC,
                "LOADBOOL", I.LoadBool), // R(A) := (bool)B; if (C) pc++
            new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgN, EOpMode.IABC,
                "LOADNIL ", I.LoadNil), // R(A), R(A+1), ..., R(A+B) := nil
            new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgN, EOpMode.IABC, "GETUPVAL",
                null), // R(A) := UpValue[B]
            new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgK, EOpMode.IABC,
                "GETTABUP", null), // R(A) := UpValue[B][RK(C)]
            new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgK, EOpMode.IABC, "GETTABLE",
                I.GetTable), // R(A) := R(B)[RK(C)]
            new OpCode(0, 0, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC,
                "SETTABUP", null), // UpValue[A][RK(B)] := RK(C)
            new OpCode(0, 0, EOpArgMask.OpArgU, EOpArgMask.OpArgN, EOpMode.IABC, "SETUPVAL",
                null), // UpValue[B] := R(A)
            new OpCode(0, 0, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC, "SETTABLE",
                I.SetTable), // R(A)[RK(B)] := RK(C)
            new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgU, EOpMode.IABC, "NEWTABLE",
                I.NewTable), // R(A) := {} (size = B,C)
            new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgK, EOpMode.IABC,
                "SELF    ", I.Self), // R(A+1) := R(B); R(A) := R(B)[RK(C)]
            new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC, "ADD     ",
                I.Add), // R(A) := RK(B) + RK(C)
            new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC, "SUB     ",
                I.Sub), // R(A) := RK(B) - RK(C)
            new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC, "MUL     ",
                I.Mul), // R(A) := RK(B) * RK(C)
            new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC, "MOD     ",
                I.Mod), // R(A) := RK(B) % RK(C)
            new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC, "POW     ",
                I.Pow), // R(A) := RK(B) ^ RK(C)
            new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC, "DIV     ",
                I.Div), // R(A) := RK(B) / RK(C)
            new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC, "IDIV    ",
                I.IDiv), // R(A) := RK(B) // RK(C)
            new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC, "BAND    ",
                I.BAnd), // R(A) := RK(B) & RK(C)
            new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC, "BOR     ",
                I.BOr), // R(A) := RK(B) | RK(C)
            new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC, "BXOR    ",
                I.BXor), // R(A) := RK(B) ~ RK(C)
            new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC, "SHL     ",
                I.Shl), // R(A) := RK(B) << RK(C)
            new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC, "SHR     ",
                I.Shr), // R(A) := RK(B) >> RK(C)
            new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IABC, "UNM     ", I.Unm), // R(A) := -R(B)
            new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IABC, "BNOT    ", I.BNot), // R(A) := ~R(B)
            new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IABC, "NOT     ", I.Not), // R(A) := not R(B)
            new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IABC, "LEN     ",
                I.Length), // R(A) := length of R(B)
            new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgR, EOpMode.IABC,
                "CONCAT  ", I.Concat), // R(A) := R(B).. ... ..R(C)
            new OpCode(0, 0, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IAsBx,
                "JMP     ", I.Jump), // pc+=sBx; if (A) close all upvalues >= R(A - 1)
            new OpCode(1, 0, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC,
                "EQ      ", I.Eq), // if ((RK(B) == RK(C)) ~= A) then pc++
            new OpCode(1, 0, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC,
                "LT      ", I.Lt), // if ((RK(B) <  RK(C)) ~= A) then pc++
            new OpCode(1, 0, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC,
                "LE      ", I.Le), // if ((RK(B) <= RK(C)) ~= A) then pc++
            new OpCode(1, 0, EOpArgMask.OpArgN, EOpArgMask.OpArgU, EOpMode.IABC,
                "TEST    ", I.Test), // if not (R(A) <=> C) then pc++
            new OpCode(1, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgU, EOpMode.IABC,
                "TESTSET ", I.TestSet), // if (R(B) <=> C) then R(A) := R(B) else pc++
            new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgU, EOpMode.IABC,
                "CALL    ", I.Call), // R(A), ... ,R(A+C-2) := R(A)(R(A+1), ... ,R(A+B-1))
            new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgU, EOpMode.IABC,
                "TAILCALL", I.TailCall), // return R(A)(R(A+1), ... ,R(A+B-1))
            new OpCode(0, 0, EOpArgMask.OpArgU, EOpArgMask.OpArgN, EOpMode.IABC,
                "RETURN  ", I.Return), // return R(A), ... ,R(A+B-2)
            new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IAsBx,
                "FORLOOP ", I.ForLoop), // R(A)+=R(A+2); if R(A) <?= R(A+1) then { pc+=sBx; R(A+3)=R(A) }
            new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IAsBx, "FORPREP ",
                I.ForPrep), // R(A)-=R(A+2); pc+=sBx
            new OpCode(0, 0, EOpArgMask.OpArgN, EOpArgMask.OpArgU, EOpMode.IABC,
                "TFORCALL", null), // R(A+3), ... ,R(A+2+C) := R(A)(R(A+1), R(A+2));
            new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IAsBx,
                "TFORLOOP", null), // if R(A+1) ~= nil then { R(A)=R(A+1); pc += sBx }
            new OpCode(0, 0, EOpArgMask.OpArgU, EOpArgMask.OpArgU, EOpMode.IABC,
                "SETLIST ", I.SetList), // R(A)[(C-1)*FPF+i] := R(A+i), 1 <= i <= B
            new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgN, EOpMode.IABx,
                "CLOSURE ", I.Closure), // R(A) := closure(KPROTO[Bx])
            new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgN, EOpMode.IABC,
                "VARARG  ", I.Vararg), // R(A), R(A+1), ..., R(A+B-2) = vararg
            new OpCode(0, 0, EOpArgMask.OpArgU, EOpArgMask.OpArgU, EOpMode.IAx,
                "EXTRAARG", null), // extra (larger) argument for previous opcode
        };
    }
}