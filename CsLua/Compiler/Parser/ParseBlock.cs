using System.Collections.Generic;
using CsLua.Compiler.Ast;
using CsLua.Compiler.Lexer;

namespace CsLua.Compiler.Parser
{
    static partial class Parser
    {
        private static Block ParseBlock(Lexer.Lexer lexer)
        {
            return new Block
            {
                Stats = ParseStats(lexer),
                RetExps = ParseRetExps(lexer),
                LastLine = lexer.Line
            };
        }

        private static List<Exp> ParseRetExps(Lexer.Lexer lexer)
        {
            if (lexer.LookAhead() != ETokenType.KwReturn)
                return null;

            lexer.NextToken(out _, out _, out _);

            var ret = new List<Exp>();

            switch (lexer.LookAhead())
            {
                case ETokenType.Eof:
                case ETokenType.KwEnd:
                case ETokenType.KwElse:
                case ETokenType.KwElseIf:
                case ETokenType.KwUntil:
                    return ret;

                case ETokenType.SepSemi:
                    lexer.NextToken(out _, out _, out _);
                    return ret;

                default:
                    var exps = ParseExpList(lexer);
                    if (lexer.LookAhead() == ETokenType.SepSemi)
                        lexer.NextToken(out _, out _, out _);
                    return exps;
            }
        }

        private static List<Stat> ParseStats(Lexer.Lexer lexer)
        {
            var stats = new List<Stat>();
            while (!IsReturnOrBlockEnd(lexer.LookAhead()))
            {
                var stat = ParseStat(lexer);
                if (!(stat is EmptyStat))
                    stats.Add(stat);
            }

            return stats;
        }

        private static bool IsReturnOrBlockEnd(ETokenType tokenType)
        {
            switch (tokenType)
            {
                case ETokenType.KwReturn:
                case ETokenType.Eof:
                case ETokenType.KwEnd:
                case ETokenType.KwElse:
                case ETokenType.KwElseIf:
                case ETokenType.KwUntil:
                    return true;
            }

            return false;
        }
    }
}