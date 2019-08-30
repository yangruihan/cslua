using System;

namespace CsLua.VM
{
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

    struct OpCode
    {
        public byte TestFlag;
        public byte SetAFlag;
        public EOpArgMask ArgBMode;
        public EOpArgMask ArgCMode;
        public EOpMode OpMode;
        public string Name;

        public OpCode(byte testFlag, byte setAFlag, EOpArgMask argBMode, EOpArgMask argCMode, EOpMode opMode,
            string name)
        {
            TestFlag = testFlag;
            SetAFlag = setAFlag;
            ArgBMode = argBMode;
            ArgCMode = argCMode;
            OpMode = opMode;
            Name = name;
        }
    }

    static class OpCodes
    {
        public static OpCode[] Codes =
        {
            	new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IABC , "MOVE    "), // R(A) := R(B)
				new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgN, EOpMode.IABx , "LOADK   "), // R(A) := Kst(Bx)
				new OpCode(0, 1, EOpArgMask.OpArgN, EOpArgMask.OpArgN, EOpMode.IABx , "LOADKX  "), // R(A) := Kst(extra arg)
				new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgU, EOpMode.IABC , "LOADBOOL"), // R(A) := (bool)B; if (C) pc++
				new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgN, EOpMode.IABC , "LOADNIL "), // R(A), R(A+1), ..., R(A+B) := nil
				new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgN, EOpMode.IABC , "GETUPVAL"), // R(A) := UpValue[B]
				new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgK, EOpMode.IABC , "GETTABUP"), // R(A) := UpValue[B][RK(C)]
				new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgK, EOpMode.IABC , "GETTABLE"), // R(A) := R(B)[RK(C)]
				new OpCode(0, 0, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "SETTABUP"), // UpValue[A][RK(B)] := RK(C)
				new OpCode(0, 0, EOpArgMask.OpArgU, EOpArgMask.OpArgN, EOpMode.IABC , "SETUPVAL"), // UpValue[B] := R(A)
				new OpCode(0, 0, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "SETTABLE"), // R(A)[RK(B)] := RK(C)
				new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgU, EOpMode.IABC , "NEWTABLE"), // R(A) := {} (size = B,C)
				new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgK, EOpMode.IABC , "SELF    "), // R(A+1) := R(B); R(A) := R(B)[RK(C)]
				new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "ADD     "), // R(A) := RK(B) + RK(C)
				new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "SUB     "), // R(A) := RK(B) - RK(C)
				new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "MUL     "), // R(A) := RK(B) * RK(C)
				new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "MOD     "), // R(A) := RK(B) % RK(C)
				new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "POW     "), // R(A) := RK(B) ^ RK(C)
				new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "DIV     "), // R(A) := RK(B) / RK(C)
				new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "IDIV    "), // R(A) := RK(B) // RK(C)
				new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "BAND    "), // R(A) := RK(B) & RK(C)
				new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "BOR     "), // R(A) := RK(B) | RK(C)
				new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "BXOR    "), // R(A) := RK(B) ~ RK(C)
				new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "SHL     "), // R(A) := RK(B) << RK(C)
				new OpCode(0, 1, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "SHR     "), // R(A) := RK(B) >> RK(C)
				new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IABC , "UNM     "), // R(A) := -R(B)
				new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IABC , "BNOT    "), // R(A) := ~R(B)
				new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IABC , "NOT     "), // R(A) := not R(B)
				new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IABC , "LEN     "), // R(A) := length of R(B)
				new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgR, EOpMode.IABC , "CONCAT  "), // R(A) := R(B).. ... ..R(C)
				new OpCode(0, 0, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IAsBx, "JMP     "), // pc+=sBx; if (A) close all upvalues >= R(A - 1)
				new OpCode(1, 0, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "EQ      "), // if ((RK(B) == RK(C)) ~= A) then pc++
				new OpCode(1, 0, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "LT      "), // if ((RK(B) <  RK(C)) ~= A) then pc++
				new OpCode(1, 0, EOpArgMask.OpArgK, EOpArgMask.OpArgK, EOpMode.IABC , "LE      "), // if ((RK(B) <= RK(C)) ~= A) then pc++
				new OpCode(1, 0, EOpArgMask.OpArgN, EOpArgMask.OpArgU, EOpMode.IABC , "TEST    "), // if not (R(A) <=> C) then pc++
				new OpCode(1, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgU, EOpMode.IABC , "TESTSET "), // if (R(B) <=> C) then R(A) := R(B) else pc++
				new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgU, EOpMode.IABC , "CALL    "), // R(A), ... ,R(A+C-2) := R(A)(R(A+1), ... ,R(A+B-1))
				new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgU, EOpMode.IABC , "TAILCALL"), // return R(A)(R(A+1), ... ,R(A+B-1))
				new OpCode(0, 0, EOpArgMask.OpArgU, EOpArgMask.OpArgN, EOpMode.IABC , "RETURN  "), // return R(A), ... ,R(A+B-2)
				new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IAsBx, "FORLOOP "), // R(A)+=R(A+2); if R(A) <?= R(A+1) then { pc+=sBx; R(A+3)=R(A) }
				new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IAsBx, "FORPREP "), // R(A)-=R(A+2); pc+=sBx
				new OpCode(0, 0, EOpArgMask.OpArgN, EOpArgMask.OpArgU, EOpMode.IABC , "TFORCALL"), // R(A+3), ... ,R(A+2+C) := R(A)(R(A+1), R(A+2));
				new OpCode(0, 1, EOpArgMask.OpArgR, EOpArgMask.OpArgN, EOpMode.IAsBx, "TFORLOOP"), // if R(A+1) ~= nil then { R(A)=R(A+1); pc += sBx }
				new OpCode(0, 0, EOpArgMask.OpArgU, EOpArgMask.OpArgU, EOpMode.IABC , "SETLIST "), // R(A)[(C-1)*FPF+i] := R(A+i), 1 <= i <= B
				new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgN, EOpMode.IABx , "CLOSURE "), // R(A) := closure(KPROTO[Bx])
				new OpCode(0, 1, EOpArgMask.OpArgU, EOpArgMask.OpArgN, EOpMode.IABC , "VARARG  "), // R(A), R(A+1), ..., R(A+B-2) = vararg
				new OpCode(0, 0, EOpArgMask.OpArgU, EOpArgMask.OpArgU, EOpMode.IAx  , "EXTRAARG"), // extra (larger) argument for previous opcode
        };
    }
}