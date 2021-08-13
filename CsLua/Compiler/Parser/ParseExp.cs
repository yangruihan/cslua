using System.Collections.Generic;
using System.Globalization;
using CsLua.API;
using CsLua.Common;
using CsLua.Compiler.Ast;
using CsLua.Compiler.Lexer;

namespace CsLua.Compiler.Parser
{
    internal static partial class Parser
    {
        private static List<Exp> ParseExpList(Lexer.Lexer lexer)
        {
            var exps = new List<Exp> {ParseExp(lexer)};

            while (lexer.LookAhead() == ETokenType.SepComma)
            {
                lexer.NextToken(out _, out _, out _);
                exps.Add(ParseExp(lexer));
            }

            return exps;
        }

        private static Exp ParseExp(Lexer.Lexer lexer)
        {
            return ParseExp12(lexer);
        }

        private static Exp ParseExp12(Lexer.Lexer lexer)
        {
            var exp = ParseExp11(lexer);
            while (lexer.LookAhead() == ETokenType.OpOr)
            {
                lexer.NextToken(out var line, out var op, out _);
                var lor = new BinopExp
                {
                    Line = line,
                    Op = op,
                    Exp1 = exp,
                    Exp2 = ParseExp11(lexer)
                };
                exp = OptimizeLogicalOr(lor);
            }

            return exp;
        }

        private static Exp ParseExp11(Lexer.Lexer lexer)
        {
            var exp = ParseExp10(lexer);
            while (lexer.LookAhead() == ETokenType.OpAnd)
            {
                lexer.NextToken(out var line, out var op, out _);
                var land = new BinopExp
                {
                    Line = line,
                    Op = op,
                    Exp1 = exp,
                    Exp2 = ParseExp10(lexer)
                };
                exp = OptimizeLogicalAnd(land);
            }

            return exp;
        }

        private static Exp ParseExp10(Lexer.Lexer lexer)
        {
            var exp = ParseExp9(lexer);
            while (true)
            {
                switch (lexer.LookAhead())
                {
                    case ETokenType.OpLt:
                    case ETokenType.OpGt:
                    case ETokenType.OpNe:
                    case ETokenType.OpLe:
                    case ETokenType.OpGe:
                    case ETokenType.OpEq:
                    {
                        lexer.NextToken(out var line, out var op, out _);
                        exp = new BinopExp
                        {
                            Line = line,
                            Op = op,
                            Exp1 = exp,
                            Exp2 = ParseExp9(lexer)
                        };
                        break;
                    }

                    default:
                        return exp;
                }
            }
        }

        private static Exp ParseExp9(Lexer.Lexer lexer)
        {
            var exp = ParseExp8(lexer);
            while (lexer.LookAhead() == ETokenType.OpBOr)
            {
                lexer.NextToken(out var line, out var op, out _);
                var bor = new BinopExp
                {
                    Line = line,
                    Op = op,
                    Exp1 = exp,
                    Exp2 = ParseExp8(lexer)
                };
                exp = OptimizeBitwiseBinaryOp(bor);
            }

            return exp;
        }

        private static Exp ParseExp8(Lexer.Lexer lexer)
        {
            var exp = ParseExp7(lexer);
            while (lexer.LookAhead() == ETokenType.OpBXor)
            {
                lexer.NextToken(out var line, out var op, out _);
                var bxor = new BinopExp
                {
                    Line = line,
                    Op = op,
                    Exp1 = exp,
                    Exp2 = ParseExp7(lexer)
                };
                exp = OptimizeBitwiseBinaryOp(bxor);
            }

            return exp;
        }

        private static Exp ParseExp7(Lexer.Lexer lexer)
        {
            var exp = ParseExp6(lexer);
            while (lexer.LookAhead() == ETokenType.OpBAnd)
            {
                lexer.NextToken(out var line, out var op, out _);
                var band = new BinopExp
                {
                    Line = line,
                    Op = op,
                    Exp1 = exp,
                    Exp2 = ParseExp6(lexer)
                };
            }

            return exp;
        }

        private static Exp ParseExp6(Lexer.Lexer lexer)
        {
            var exp = ParseExp5(lexer);
            while (true)
            {
                switch (lexer.LookAhead())
                {
                    case ETokenType.OpShl:
                    case ETokenType.OpShr:
                    {
                        lexer.NextToken(out var line, out var op, out _);
                        var shx = new BinopExp
                        {
                            Line = line,
                            Op = op,
                            Exp1 = exp,
                            Exp2 = ParseExp5(lexer)
                        };
                        exp = OptimizeBitwiseBinaryOp(shx);
                        break;
                    }

                    default:
                        return exp;
                }
            }
        }

        private static Exp ParseExp5(Lexer.Lexer lexer)
        {
            var exp = ParseExp4(lexer);
            if (lexer.LookAhead() != ETokenType.OpConcat)
                return exp;

            var line = 0;
            var exps = new List<Exp> {exp};
            while (lexer.LookAhead() == ETokenType.OpConcat)
            {
                lexer.NextToken(out line, out _, out _);
                exps.Add(ParseExp4(lexer));
            }

            return new ConcatExp
            {
                Line = line,
                Exps = exps
            };
        }

        private static Exp ParseExp4(Lexer.Lexer lexer)
        {
            var exp = ParseExp3(lexer);
            while (true)
            {
                switch (lexer.LookAhead())
                {
                    case ETokenType.OpAdd:
                    case ETokenType.OpSub:
                    {
                        lexer.NextToken(out var line, out var op, out _);
                        var arith = new BinopExp
                        {
                            Line = line,
                            Op = op,
                            Exp1 = exp,
                            Exp2 = ParseExp3(lexer)
                        };
                        exp = OptimizeArithBinaryOp(arith);
                        break;
                    }

                    default:
                        return exp;
                }
            }
        }

        private static Exp ParseExp3(Lexer.Lexer lexer)
        {
            var exp = ParseExp2(lexer);
            while (true)
            {
                switch (lexer.LookAhead())
                {
                    case ETokenType.OpMul:
                    case ETokenType.OpMod:
                    case ETokenType.OpDiv:
                    case ETokenType.OpIDiv:
                    {
                        lexer.NextToken(out var line, out var op, out _);
                        var arith = new BinopExp
                        {
                            Line = line,
                            Op = op,
                            Exp1 = exp,
                            Exp2 = ParseExp2(lexer)
                        };
                        exp = OptimizeArithBinaryOp(arith);
                        break;
                    }

                    default:
                        return exp;
                }
            }
        }

        private static Exp ParseExp2(Lexer.Lexer lexer)
        {
            switch (lexer.LookAhead())
            {
                case ETokenType.OpUnm:
                case ETokenType.OpBNot:
                case ETokenType.OpLen:
                case ETokenType.OpNot:
                {
                    lexer.NextToken(out var line, out var op, out _);
                    var exp = new UnopExp
                    {
                        Line = line,
                        Op = op,
                        Exp = ParseExp2(lexer)
                    };

                    return OptimizeUnaryOp(exp);
                }
            }

            return ParseExp1(lexer);
        }

        private static Exp ParseExp1(Lexer.Lexer lexer)
        {
            var exp = ParseExp0(lexer);
            if (lexer.LookAhead() == ETokenType.OpPow)
            {
                lexer.NextToken(out var line, out var op, out _);
                exp = new BinopExp
                {
                    Line = line,
                    Op = op,
                    Exp1 = exp,
                    Exp2 = ParseExp2(lexer)
                };
            }

            return OptimizePow(exp);
        }

        private static Exp ParseExp0(Lexer.Lexer lexer)
        {
            switch (lexer.LookAhead())
            {
                case ETokenType.Vararg:
                {
                    lexer.NextToken(out var line, out _, out _);
                    return new VarargExp {Line = line};
                }

                case ETokenType.KwNil:
                {
                    lexer.NextToken(out var line, out _, out _);
                    return new NilExp {Line = line};
                }

                case ETokenType.KwTrue:
                {
                    lexer.NextToken(out var line, out _, out _);
                    return new TrueExp {Line = line};
                }

                case ETokenType.KwFalse:
                {
                    lexer.NextToken(out var line, out _, out _);
                    return new FalseExp {Line = line};
                }

                case ETokenType.String:
                {
                    lexer.NextToken(out var line, out _, out var token);
                    return new StringExp {Line = line, Str = token};
                }

                case ETokenType.Number:
                {
                    return ParseNumberExp(lexer);
                }

                case ETokenType.SepLCurly:
                {
                    return ParseTableConstructorExp(lexer);
                }

                case ETokenType.KwFunction:
                {
                    lexer.NextToken(out _, out _, out _);
                    return ParseFuncDefExp(lexer);
                }

                default:
                    return ParsePrefixExp(lexer);
            }
        }

        private static Exp ParseTableConstructorExp(Lexer.Lexer lexer)
        {
            var line = lexer.Line;
            lexer.NextTokenOfKind(ETokenType.SepLCurly, out _, out _);
            ParseFieldList(lexer, out var keyExps, out var valExps);
            lexer.NextTokenOfKind(ETokenType.SepRCurly, out _, out _);
            var lastLine = lexer.Line;
            return new TableConstructorExp
            {
                Line = line,
                LastLine = lastLine,
                KeyExps = keyExps,
                ValExps = valExps
            };
        }

        private static void ParseFieldList(Lexer.Lexer lexer, out List<Exp> ks, out List<Exp> vs)
        {
            ks = null;
            vs = null;

            if (lexer.LookAhead() != ETokenType.SepRCurly)
            {
                ParseField(lexer, out var k, out var v);

                ks = new List<Exp> {k};
                vs = new List<Exp> {v};

                while (IsFieldSep(lexer.LookAhead()))
                {
                    lexer.NextToken(out _, out _, out _);
                    if (lexer.LookAhead() != ETokenType.SepRCurly)
                    {
                        ParseField(lexer, out k, out v);
                        ks.Add(k);
                        vs.Add(v);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private static void ParseField(Lexer.Lexer lexer, out Exp k, out Exp v)
        {
            k = null;
            v = null;

            if (lexer.LookAhead() == ETokenType.SepLBracket)
            {
                lexer.NextToken(out _, out _, out _);
                k = ParseExp(lexer);
                lexer.NextTokenOfKind(ETokenType.SepRBracket, out _, out _);
                lexer.NextTokenOfKind(ETokenType.OpAssign, out _, out _);
                v = ParseExp(lexer);
                return;
            }

            var exp = ParseExp(lexer);
            if (exp is NameExp nameExp)
            {
                if (lexer.LookAhead() == ETokenType.OpAssign)
                {
                    lexer.NextToken(out _, out _, out _);
                    k = new StringExp {Line = nameExp.Line, Str = nameExp.Name};
                    v = ParseExp(lexer);
                    return;
                }
            }

            v = exp;
        }

        private static bool IsFieldSep(ETokenType kind)
        {
            return kind == ETokenType.SepComma || kind == ETokenType.SepSemi;
        }

        private static FuncDefExp ParseFuncDefExp(Lexer.Lexer lexer)
        {
            var line = lexer.Line;
            lexer.NextTokenOfKind(ETokenType.SepLParen, out _, out _);
            ParseParList(lexer, out var parList, out var isVararg);
            lexer.NextTokenOfKind(ETokenType.SepRParen, out _, out _);
            var block = ParseBlock(lexer);
            lexer.NextTokenOfKind(ETokenType.KwEnd, out var lastLine, out _);
            return new FuncDefExp
            {
                Line = line,
                LastLine = lastLine,
                ParList = parList,
                IsVararg = isVararg,
                Block = block
            };
        }

        private static void ParseParList(Lexer.Lexer lexer, out List<string> parList, out bool isVararg)
        {
            parList = new List<string>();
            isVararg = false;

            switch (lexer.LookAhead())
            {
                case ETokenType.SepRParen:
                    return;

                case ETokenType.Vararg:
                {
                    lexer.NextToken(out _, out _, out _);
                    isVararg = true;
                    return;
                }
            }

            lexer.NextIdentifier(out _, out var name);
            parList.Add(name);

            while (lexer.LookAhead() == ETokenType.SepComma)
            {
                lexer.NextToken(out _, out _, out _);
                if (lexer.LookAhead() == ETokenType.Identifier)
                {
                    lexer.NextIdentifier(out _, out name);
                    parList.Add(name);
                }
                else
                {
                    lexer.NextTokenOfKind(ETokenType.Vararg, out _, out _);
                    isVararg = true;
                    break;
                }
            }
        }

        private static Exp ParseNumberExp(Lexer.Lexer lexer)
        {
            lexer.NextToken(out var line, out _, out var token);

            var numberStyle = NumberStyles.Number;
            var e = 1.0;

            if (token.StartsWith("0x"))
            {
                token = token.Substring(2);
                if (token.Contains("p"))
                {
                    var eIdx = token.IndexOf("p");
                    var t = LuaFloat.Parse("1" + token.Substring(eIdx).Replace("p", "e"));
                    var d = t > 1 ? 0.1 : 10;
                    while (t != 1)
                    {
                        e *= d > 1 ? 0.5 : 2;
                        t *= d;
                    }

                    token = token.Substring(0, eIdx);
                }

                numberStyle = NumberStyles.HexNumber;
            }

            if (LuaInt.TryParse(token, numberStyle, new NumberFormatInfo(), out var result))
            {
                if (e != 1.0)
                    return new FloatExp {Line = line, Val = result * e};

                return new IntegerExp {Line = line, Val = result};
            }

            if (LuaFloat.TryParse(token, out var fResult))
                return new FloatExp {Line = line, Val = fResult};

            Debug.Panic("not a number: " + token);
            return null;
        }
    }
}