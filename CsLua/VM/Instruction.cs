using System;
using System.Runtime.InteropServices;
using System.Text;
using CsLua.API;
using CsLua.Misc;

namespace CsLua.VM
{
    /*
      指令结构

     31       22       13       5    0
      +-------+^------+-^-----+-^-----
      |b=9bits |c=9bits |a=8bits|op=6| iabc
      +-------+^------+-^-----+-^-----
      |    bx=18bits    |a=8bits|op=6| iabx
      +-------+^------+-^-----+-^-----
      |   sbx=18bits    |a=8bits|op=6| iasbx
      +-------+^------+-^-----+-^-----
      |    ax=26bits            |op=6| iax
      +-------+^------+-^-----+-^-----
     31      23      15       7      0
    */
    [StructLayout(LayoutKind.Sequential)]
    internal struct Instruction
    {
        public const int MAXARG_Bx = (1 << 18) - 1;
        public const int MAXARG_sBx = MAXARG_Bx >> 1;

        public static Instruction[] FromIntArr(UInt32[] ins)
        {
            var ret = new Instruction[ins.Length];
            for (var i = 0; i < ins.Length; i++)
                ret[i] = ins[i];
            return ret;
        }

        private UInt32 _data;

        public static implicit operator Instruction(UInt32 data)
        {
            return new Instruction(data);
        }

        public static implicit operator UInt32(Instruction instruction)
        {
            return instruction._data;
        }

        public int A => (int)(_data >> 6 & 0xff);

        public Instruction(UInt32 data)
        {
            this._data = data;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var mode = this.OpMode();
            var name = this.OpName();
            sb.Append($"{name:-10}\t");

            if (mode == EOpMode.IABC)
            {
                ABC(out var a, out var b, out var c);
                sb.Append(a);

                if (BMode() != EOpArgMask.OpArgN)
                {
                    if (b > 0xff)
                        sb.Append(' ').Append(-1 - (b & 0xff));
                    else
                        sb.Append(' ').Append(b);
                }

                if (CMode() != EOpArgMask.OpArgN)
                {
                    if (c > 0xff)
                        sb.Append(' ').Append(-1 - (c & 0xff));
                    else
                        sb.Append(' ').Append(c);
                }
            }
            else if (mode == EOpMode.IABx)
            {
                ABx(out var a, out var bx);
                sb.Append(a);

                if (BMode() == EOpArgMask.OpArgK)
                    sb.Append(' ').Append(-1 - bx);
                else if (BMode() == EOpArgMask.OpArgU)
                    sb.Append(' ').Append(bx);
            }
            else if (mode == EOpMode.IAsBx)
            {
                AsBx(out var a, out var sbx);
                sb.Append(a).Append(' ').Append(sbx);
            }
            else
            {
                Ax(out var ax);
                sb.Append(-1 - ax);
            }

            return sb.ToString();
        }

        public EOpCode Opcode()
        {
            return (EOpCode)(_data & 0x3f);
        }

        public void ABC(out int a, out int b, out int c)
        {
            a = (int)(_data >> 6 & 0xff);
            c = (int)(_data >> 14 & 0x1ff);
            b = (int)(_data >> 23 & 0x1ff);
        }

        public void ABx(out int a, out int bx)
        {
            a = (int)(_data >> 6 & 0xff);
            bx = (int)(_data >> 14);
        }

        public void AsBx(out int a, out int sbx)
        {
            ABx(out a, out sbx);
            sbx -= MAXARG_sBx;
        }

        public void Ax(out int a)
        {
            a = (int)(_data >> 6);
        }

        public string OpName()
        {
            return OpCodes.Codes[(int)Opcode()].Name;
        }

        public EOpMode OpMode()
        {
            return OpCodes.Codes[(int)Opcode()].OpMode;
        }

        public EOpArgMask BMode()
        {
            return OpCodes.Codes[(int)Opcode()].ArgBMode;
        }

        public EOpArgMask CMode()
        {
            return OpCodes.Codes[(int)Opcode()].ArgCMode;
        }

        public void Execute(ILuaVM luaVM)
        {
            var action = OpCodes.Codes[(int)Opcode()].Action;
            if (!(action is null))
                action(this, luaVM);
            else
                luaVM.Error(OpName());
        }
    }
}