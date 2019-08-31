using System;
using CsLua.API;
using CsLua.Common;

namespace CsLua.VM
{
    /*
     31       22       13       5    0
      +-------+^------+-^-----+-^-----
      |b=9bits |c=9bits |a=8bits|op=6|
      +-------+^------+-^-----+-^-----
      |    bx=18bits    |a=8bits|op=6|
      +-------+^------+-^-----+-^-----
      |   sbx=18bits    |a=8bits|op=6|
      +-------+^------+-^-----+-^-----
      |    ax=26bits            |op=6|
      +-------+^------+-^-----+-^-----
     31      23      15       7      0
    */
    struct Instruction
    {
        private const int MAXARG_Bx = (1 << 18) - 1;
        private const int MAXARG_sBx = MAXARG_Bx >> 1;

        private UInt32 _data;

        public static implicit operator Instruction(UInt32 data)
        {
            return new Instruction(data);
        }

        public static implicit operator UInt32(Instruction instruction)
        {
            return instruction._data;
        }

        public Instruction(UInt32 data)
        {
            this._data = data;
        }

        public EOpCode Opcode()
        {
            return (EOpCode) (_data & 0x3f);
        }

        public void ABC(out int a, out int b, out int c)
        {
            a = (int) (_data >> 6 & 0xff);
            c = (int) (_data >> 14 & 0x1ff);
            b = (int) (_data >> 23 & 0x1ff);
        }

        public void ABx(out int a, out int bx)
        {
            a = (int) (_data >> 6 & 0xff);
            bx = (int) (_data >> 14);
        }

        public void AsBx(out int a, out int sbx)
        {
            ABx(out a, out sbx);
            sbx -= MAXARG_sBx;
        }

        public void Ax(out int a)
        {
            a = (int) (_data >> 6);
        }

        public string OpName()
        {
            return OpCodes.Codes[(int) Opcode()].Name;
        }

        public EOpMode OpMode()
        {
            return OpCodes.Codes[(int) Opcode()].OpMode;
        }

        public EOpArgMask BMode()
        {
            return OpCodes.Codes[(int) Opcode()].ArgBMode;
        }

        public EOpArgMask CMode()
        {
            return OpCodes.Codes[(int) Opcode()].ArgCMode;
        }

        public void Execute(ILuaVM luaVM)
        {
            var action = OpCodes.Codes[(int) Opcode()].Action;
            if (!(action is null))
                action(this, luaVM);
            else
                Debug.Panic(OpName());
        }
    }
}