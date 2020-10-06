using System;
using CsLua.Compiler.Ast;
using CsLua.Compiler.Lexer;
using CsLua.Number;

namespace CsLua.Compiler.Parser
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    static partial class Parser
    {
        private static Exp OptimizeLogicalOr(BinopExp exp)
        {
            if (IsTrue(exp.Exp1))
                return exp.Exp1;

            if (IsFalse(exp.Exp1))
                return exp.Exp2;

            return exp;
        }

        private static Exp OptimizeLogicalAnd(BinopExp exp)
        {
            if (IsFalse(exp.Exp1))
                return exp.Exp1;

            if (IsTrue(exp.Exp1) && !IsVarargOrFuncCall(exp.Exp2))
                return exp.Exp2;

            return exp;
        }

        private static Exp OptimizeBitwiseBinaryOp(BinopExp exp)
        {
            if (CastToInt(exp.Exp1, out var i1)
                && CastToInt(exp.Exp2, out var i2))
            {
                switch (exp.Op)
                {
                    case ETokenType.OpBAnd:
                        return new IntegerExp
                        {
                            Line = exp.Line,
                            Val = i1 & i2
                        };

                    case ETokenType.OpBOr:
                        return new IntegerExp
                        {
                            Line = exp.Line,
                            Val = i1 | i2
                        };

                    case ETokenType.OpBXor:
                        return new IntegerExp
                        {
                            Line = exp.Line,
                            Val = i1 ^ i2
                        };

                    case ETokenType.OpShl:
                        return new IntegerExp
                        {
                            Line = exp.Line,
                            Val = LuaMath.ShiftLeft(i1, i2)
                        };

                    case ETokenType.OpShr:
                        return new IntegerExp
                        {
                            Line = exp.Line,
                            Val = LuaMath.ShiftRight(i1, i2)
                        };
                }
            }

            return exp;
        }

        private static Exp OptimizeArithBinaryOp(BinopExp exp)
        {
            if (exp.Exp1 is IntegerExp ie1
                && exp.Exp2 is IntegerExp ie2)
            {
                switch (exp.Op)
                {
                    case ETokenType.OpAdd:
                        return new IntegerExp
                        {
                            Line = exp.Line,
                            Val = ie1.Val + ie2.Val
                        };

                    case ETokenType.OpSub:
                        return new IntegerExp
                        {
                            Line = exp.Line,
                            Val = ie1.Val - ie2.Val
                        };

                    case ETokenType.OpMul:
                        return new IntegerExp
                        {
                            Line = exp.Line,
                            Val = ie1.Val * ie2.Val
                        };

                    case ETokenType.OpIDiv:
                    {
                        if (ie2.Val != 0)
                        {
                            return new IntegerExp
                            {
                                Line = exp.Line,
                                Val = LuaMath.IFloorDiv(ie1.Val, ie2.Val)
                            };
                        }

                        break;
                    }

                    case ETokenType.OpMod:
                    {
                        if (ie2.Val != 0)
                        {
                            return new IntegerExp
                            {
                                Line = exp.Line,
                                Val = LuaMath.IMod(ie1.Val, ie2.Val)
                            };
                        }

                        break;
                    }
                }
            }

            if (CastToFloat(exp.Exp1, out var f)
                && CastToFloat(exp.Exp2, out var g))
            {
                switch (exp.Op)
                {
                    case ETokenType.OpAdd:
                        return new FloatExp
                        {
                            Line = exp.Line,
                            Val = f + g
                        };

                    case ETokenType.OpSub:
                        return new FloatExp
                        {
                            Line = exp.Line,
                            Val = f - g
                        };

                    case ETokenType.OpMul:
                        return new FloatExp
                        {
                            Line = exp.Line,
                            Val = f * g
                        };

                    case ETokenType.OpDiv:
                    {
                        if (g != 0)
                        {
                            return new FloatExp
                            {
                                Line = exp.Line,
                                Val = f / g,
                            };
                        }

                        break;
                    }

                    case ETokenType.OpIDiv:
                    {
                        if (g != 0)
                        {
                            return new FloatExp
                            {
                                Line = exp.Line,
                                Val = LuaMath.FFloorDiv(f, g)
                            };
                        }

                        break;
                    }

                    case ETokenType.OpMod:
                    {
                        if (g != 0)
                        {
                            return new FloatExp
                            {
                                Line = exp.Line,
                                Val = LuaMath.FMod(f, g)
                            };
                        }

                        break;
                    }

                    case ETokenType.OpPow:
                        return new FloatExp
                        {
                            Line = exp.Line,
                            Val = Math.Pow(f, g)
                        };
                }
            }

            return exp;
        }

        private static Exp OptimizePow(Exp exp)
        {
            if (exp is BinopExp be)
            {
                if (be.Op == ETokenType.OpPow)
                    be.Exp2 = OptimizePow(be.Exp2);

                return OptimizeArithBinaryOp(be);
            }

            return exp;
        }

        private static Exp OptimizeUnaryOp(UnopExp exp)
        {
            switch (exp.Op)
            {
                case ETokenType.OpUnm:
                    return OptimizeUnm(exp);

                case ETokenType.OpNot:
                    return OptimizeNot(exp);

                case ETokenType.OpBNot:
                    return OptimizeBnot(exp);

                default:
                    return exp;
            }
        }

        private static Exp OptimizeUnm(UnopExp exp)
        {
            switch (exp.Exp)
            {
                case IntegerExp ie:
                    ie.Val = -ie.Val;
                    return ie;
                case FloatExp fe:
                    fe.Val = -fe.Val;
                    return fe;
                default:
                    return exp;
            }
        }

        private static Exp OptimizeNot(UnopExp exp)
        {
            switch (exp.Exp)
            {
                case NilExp ne:
                case FalseExp fe:
                    return new TrueExp {Line = exp.Line};

                case TrueExp te:
                case IntegerExp ie:
                case FloatExp fe:
                case StringExp se:
                    return new FalseExp {Line = exp.Line};

                default:
                    return exp;
            }
        }

        private static Exp OptimizeBnot(UnopExp exp)
        {
            switch (exp.Exp)
            {
                case IntegerExp ie:
                {
                    ie.Val = ~ie.Val;
                    return ie;
                }

                case FloatExp fe:
                {
                    if (LuaMath.FloatToInteger(fe.Val, out var ret))
                    {
                        return new IntegerExp
                        {
                            Line = exp.Line,
                            Val = ~ret
                        };
                    }

                    break;
                }
            }

            return exp;
        }

        private static bool IsTrue(Exp exp)
        {
            return exp is TrueExp
                   || exp is IntegerExp
                   || exp is FloatExp
                   || exp is StringExp;
        }

        private static bool IsFalse(Exp exp)
        {
            return exp is FalseExp
                   || exp is NilExp;
        }

        private static bool IsVarargOrFuncCall(Exp exp)
        {
            return exp is VarargExp
                   || exp is FuncCallExp;
        }

        private static bool CastToInt(Exp exp, out LuaInt ret)
        {
            ret = 0;
            switch (exp)
            {
                case IntegerExp ie:
                {
                    ret = ie.Val;
                    return true;
                }

                case FloatExp fe:
                {
                    return LuaMath.FloatToInteger(fe.Val, out ret);
                }

                default:
                    return false;
            }
        }

        private static bool CastToFloat(Exp exp, out LuaFloat ret)
        {
            ret = 0;
            switch (exp)
            {
                case IntegerExp ie:
                {
                    ret = ie.Val;
                    return true;
                }

                case FloatExp fe:
                {
                    ret = fe.Val;
                    return true;
                }

                default:
                    return false;
            }
        }
    }
}