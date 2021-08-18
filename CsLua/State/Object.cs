using CsLua.API;

namespace CsLua.State
{
    internal partial class LuaState
    {
        private void SetValue(int idx, int idx2)
        {
            GetValueByRelIdx(idx, out var absIdx);
            GetValueByRelIdx(idx2, out var absIdx2);
            Stack[absIdx] = Stack[absIdx2];
        }

        private void SetValue(int idx, LuaValue? v)
        {
            GetValueByRelIdx(idx, out var absIdx);
            SetValueAbsIdx(absIdx, v);
        }

        private void SetValueAbsIdx(int absIdx, LuaValue? v)
        {
            Stack[absIdx] = v;
        }

        private void SetValueAbsIdx(int absIdx, int absIdx2)
        {
            Stack[absIdx] = Stack[absIdx2];
        }

        private void InnerArith(EArithOp op, int p1, int p2, int res)
        {
            var oper = Operators.Ops[(int)op];
            switch (op)
            {
                // operate only on integers
                case EArithOp.BAnd:
                case EArithOp.BOr:
                case EArithOp.BXor:
                case EArithOp.Shl:
                case EArithOp.Shr:
                case EArithOp.BNot:
                {
                    if (InnerToInteger(p1, out var i1)
                        && InnerToInteger(p2, out var i2))
                    {
                        SetValue(res, LuaValue.Create(oper.IntegerFunc!(i1, i2)));
                        return;
                    }

                    break;
                }

                // operate only on floats
                case EArithOp.Div:
                case EArithOp.Pow:
                {
                    if (InnerToNumber(p1, out var n1)
                        && InnerToNumber(p2, out var n2))
                    {
                        SetValue(res, LuaValue.Create(oper.FloatFunc!(n1, n2)));
                        return;
                    }

                    break;
                }

                // other operations
                default:
                {
                    if (InnerToInteger(p1, out var i1)
                        && InnerToInteger(p2, out var i2))
                    {
                        SetValue(res, LuaValue.Create(oper.IntegerFunc!(i1, i2)));
                        return;
                    }

                    if (InnerToNumber(p1, out var n1)
                        && InnerToNumber(p2, out var n2))
                    {
                        SetValue(res, LuaValue.Create(oper.FloatFunc!(n1, n2)));
                        return;
                    }

                    break;
                }
            }

            TryBinTM(p1, p2, res, (ETagMethods)((int)op - (int)EArithOp.Add + (int)ETagMethods.ADD));
        }
    }
}