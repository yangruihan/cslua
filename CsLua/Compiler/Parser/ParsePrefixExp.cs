using System.Collections.Generic;
using CsLua.Compiler.Ast;
using CsLua.Compiler.Lexer;

namespace CsLua.Compiler.Parser
{
    partial class Parser
    {
        private static Exp ParsePrefixExp(Lexer.Lexer lexer)
        {
            Exp exp;
            if (lexer.LookAhead() == ETokenType.Identifier)
            {
                lexer.NextIdentifier(out var line, out var name);
                exp = new NameExp
                {
                    Line = line,
                    Name = name
                };
            }
            else
            {
                exp = ParseParensExp(lexer);
            }

            return FinishPrefixExp(lexer, exp);
        }

        private static Exp ParseParensExp(Lexer.Lexer lexer)
        {
            lexer.NextTokenOfKind(ETokenType.SepLParen, out _, out _);
            var exp = ParseExp(lexer);
            lexer.NextTokenOfKind(ETokenType.SepRParen, out _, out _);
            if (exp is VarargExp || exp is FuncCallExp || exp is NameExp || exp is TableAccessExp)
                return new ParensExp {Exp = exp};

            return exp;
        }

        private static Exp FinishPrefixExp(Lexer.Lexer lexer, Exp exp)
        {
            while (true)
            {
                switch (lexer.LookAhead())
                {
                    case ETokenType.SepLBrack:
                    {
                        lexer.NextToken(out _, out _, out _);
                        var keyExp = ParseExp(lexer);
                        lexer.NextTokenOfKind(ETokenType.SepRBrack, out _, out _);
                        exp = new TableAccessExp
                        {
                            LastLine = lexer.Line,
                            PrefixExp = exp,
                            KeyExp = keyExp
                        };
                        break;
                    }

                    case ETokenType.SepDot:
                    {
                        lexer.NextToken(out _, out _, out _);
                        lexer.NextIdentifier(out var line, out var name);
                        var keyExp = new StringExp
                        {
                            Line = line,
                            Str = name
                        };
                        exp = new TableAccessExp
                        {
                            LastLine = line,
                            PrefixExp = exp,
                            KeyExp = keyExp
                        };
                        break;
                    }

                    case ETokenType.SepColon:
                    case ETokenType.SepLParen:
                    case ETokenType.SepLCurly:
                    case ETokenType.String:
                    {
                        exp = FinishFuncCallExp(lexer, exp);
                        break;
                    }

                    default:
                        return exp;
                }
            }
        }

        private static Exp FinishFuncCallExp(Lexer.Lexer lexer, Exp prefixExp)
        {
            var nameExp = ParseNameExp(lexer);
            var line = lexer.Line;
            var args = ParseArgs(lexer);
            var lastLine = lexer.Line;
            return new FuncCallExp
            {
                Line = line,
                LastLine = lastLine,
                PrefixExp = prefixExp,
                NameExp = nameExp,
                Args = args
            };
        }

        private static List<Exp> ParseArgs(Lexer.Lexer lexer)
        {
            List<Exp> ret = null;

            switch (lexer.LookAhead())
            {
                case ETokenType.SepLParen:
                {
                    lexer.NextToken(out _, out _, out _);
                    if (lexer.LookAhead() != ETokenType.SepRParen)
                        ret = ParseExpList(lexer);

                    lexer.NextTokenOfKind(ETokenType.SepRParen, out _, out _);
                    break;
                }

                case ETokenType.SepLCurly:
                {
                    ret = new List<Exp> {ParseTableConstructorExp(lexer)};
                    break;
                }

                default:
                {
                    lexer.NextTokenOfKind(ETokenType.String, out var line, out var str);
                    ret = new List<Exp>
                    {
                        new StringExp
                        {
                            Line = line,
                            Str = str
                        }
                    };
                    break;
                }
            }

            return ret;
        }

        private static StringExp ParseNameExp(Lexer.Lexer lexer)
        {
            if (lexer.LookAhead() == ETokenType.SepColon)
            {
                lexer.NextToken(out _, out _, out _);
                lexer.NextIdentifier(out var line, out var name);
                return new StringExp
                {
                    Line = line,
                    Str = name,
                };
            }

            return null;
        }
    }
}