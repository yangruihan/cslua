using System.Collections.Generic;
using CsLua.Common;
using CsLua.Compiler.Ast;
using CsLua.Compiler.Lexer;

namespace CsLua.Compiler.Parser
{
    partial class Parser
    {
        private static Stat ParseStat(Lexer.Lexer lexer)
        {
            switch (lexer.LookAhead())
            {
                case ETokenType.SepSemi:
                    return ParseEmptyStat(lexer);

                case ETokenType.KwBreak:
                    return ParseBreakStat(lexer);

                case ETokenType.SepLabel:
                    return ParseLabelStat(lexer);

                case ETokenType.KwGoto:
                    return ParseGotoStat(lexer);

                case ETokenType.KwDo:
                    return ParseDoStat(lexer);

                case ETokenType.KwWhile:
                    return ParseWhileStat(lexer);

                case ETokenType.KwRepeat:
                    return ParseRepeatStat(lexer);

                case ETokenType.KwIf:
                    return ParseIfStat(lexer);

                case ETokenType.KwFor:
                    return ParseForStat(lexer);

                case ETokenType.KwFunction:
                    return ParseFuncDefStat(lexer);

                case ETokenType.KwLocal:
                    return ParseLocalAssignOrFuncDefStat(lexer);

                default:
                    return ParseAssignOrFuncCallStat(lexer);
            }
        }

        private static Stat ParseFuncDefStat(Lexer.Lexer lexer)
        {
            lexer.NextTokenOfKind(ETokenType.KwFunction, out _, out _);
            ParseFuncName(lexer, out var fnExp, out var hasColon);
            var fdExp = ParseFuncDefExp(lexer);

            if (hasColon)
            {
                fdExp.ParList.Insert(0, "self");
            }

            return new AssignStat
            {
                LastLine = fdExp.Line,
                VarList = new List<Exp> {fnExp},
                ExpList = new List<Exp> {fdExp},
            };
        }

        private static void ParseFuncName(Lexer.Lexer lexer, out Exp exp, out bool hasColon)
        {
            hasColon = false;
            lexer.NextIdentifier(out var line, out var name);
            exp = new NameExp {Line = line, Name = name};

            while (lexer.LookAhead() == ETokenType.SepDot)
            {
                lexer.NextToken(out _, out _, out _);
                lexer.NextIdentifier(out line, out name);
                var idx = new StringExp {Line = line, Str = name};
                exp = new TableAccessExp
                {
                    LastLine = line,
                    KeyExp = idx,
                    PrefixExp = exp,
                };
            }

            if (lexer.LookAhead() == ETokenType.SepColon)
            {
                lexer.NextToken(out _, out _, out _);
                lexer.NextIdentifier(out line, out name);
                var idx = new StringExp {Line = line, Str = name};
                exp = new TableAccessExp
                {
                    LastLine = line,
                    PrefixExp = exp,
                    KeyExp = idx,
                };
                hasColon = true;
            }
        }

        private static Stat ParseAssignOrFuncCallStat(Lexer.Lexer lexer)
        {
            var prefixExp = ParsePrefixExp(lexer);
            if (prefixExp is FuncCallExp fc)
                return (FuncCallStat) fc;
            else
                return ParseAssignStat(lexer, prefixExp);
        }

        private static Stat ParseAssignStat(Lexer.Lexer lexer, Exp prefixExp)
        {
            var varList = FinishVarList(lexer, prefixExp);
            lexer.NextTokenOfKind(ETokenType.OpAssign, out _, out _);
            var expList = ParseExpList(lexer);
            var lastLine = lexer.Line;
            return new AssignStat
            {
                LastLine = lastLine,
                ExpList = expList,
                VarList = varList
            };
        }

        private static List<Exp> FinishVarList(Lexer.Lexer lexer, Exp prefixExp)
        {
            var vars = new List<Exp> {CheckVar(lexer, prefixExp)};
            while (lexer.LookAhead() == ETokenType.SepComma)
            {
                lexer.NextToken(out _, out _, out _);
                var exp = ParsePrefixExp(lexer);
                vars.Add(CheckVar(lexer, exp));
            }

            return vars;
        }

        private static Exp CheckVar(Lexer.Lexer lexer, Exp exp)
        {
            if (exp is NameExp || exp is TableAccessExp)
                return exp;

            lexer.NextTokenOfKind(ETokenType.Unknown, out _, out _);
            Debug.Panic("unreachable!");
            return null;
        }

        private static Stat ParseLocalAssignOrFuncDefStat(Lexer.Lexer lexer)
        {
            lexer.NextTokenOfKind(ETokenType.KwLocal, out _, out _);
            if (lexer.LookAhead() == ETokenType.KwFunction)
                return FinishLocalFunctionDefStat(lexer);
            else
                return FinishLocalVarDeclStat(lexer);
        }

        private static Stat FinishLocalVarDeclStat(Lexer.Lexer lexer)
        {
            lexer.NextIdentifier(out _, out var name0);
            var nameList = FinishNameList(lexer, name0);
            List<Exp> expList = null;
            if (lexer.LookAhead() == ETokenType.OpAssign)
            {
                lexer.NextToken(out _, out _, out _);
                expList = ParseExpList(lexer);
            }

            var lastLine = lexer.Line;
            return new LocalVarDeclStat
            {
                ExpList = expList,
                LastLine = lastLine,
                NameList = nameList
            };
        }

        private static Stat FinishLocalFunctionDefStat(Lexer.Lexer lexer)
        {
            lexer.NextTokenOfKind(ETokenType.KwFunction, out _, out _);
            lexer.NextIdentifier(out _, out var name);
            var fdExp = ParseFuncDefExp(lexer);
            return new LocalFuncDefStat
            {
                Name = name,
                Exp = fdExp
            };
        }

        private static Stat ParseForStat(Lexer.Lexer lexer)
        {
            lexer.NextTokenOfKind(ETokenType.KwFor, out var lineOfToken, out _);
            lexer.NextIdentifier(out _, out var name);
            if (lexer.LookAhead() == ETokenType.OpAssign)
                return FinishForNumStat(lexer, lineOfToken, name);
            else
                return FinishForInStat(lexer, name);
        }

        private static Stat FinishForInStat(Lexer.Lexer lexer, string name)
        {
            var nameList = FinishNameList(lexer, name);
            lexer.NextTokenOfKind(ETokenType.KwIn, out _, out _);
            var expList = ParseExpList(lexer);
            lexer.NextTokenOfKind(ETokenType.KwDo, out var lineOfDo, out _);
            var block = ParseBlock(lexer);
            lexer.NextTokenOfKind(ETokenType.KwEnd, out _, out _);
            return new ForInStat
            {
                LineOfDo = lineOfDo,
                Block = block,
                ExpList = expList,
                NameList = nameList
            };
        }

        private static List<string> FinishNameList(Lexer.Lexer lexer, string name)
        {
            var names = new List<string> {name};
            while (lexer.LookAhead() == ETokenType.SepComma)
            {
                lexer.NextToken(out _, out _, out _);
                lexer.NextIdentifier(out _, out var tokenName);
                names.Add(tokenName);
            }

            return names;
        }

        private static Stat FinishForNumStat(Lexer.Lexer lexer, int lineOfToken, string name)
        {
            lexer.NextTokenOfKind(ETokenType.OpAssign, out _, out _);
            var initExp = ParseExp(lexer);
            lexer.NextTokenOfKind(ETokenType.SepComma, out _, out _);
            var limitExp = ParseExp(lexer);

            Exp stepExp;
            if (lexer.LookAhead() == ETokenType.SepComma)
            {
                lexer.NextToken(out _, out _, out _);
                stepExp = ParseExp(lexer);
            }
            else
            {
                stepExp = new IntegerExp {Line = lexer.Line, Val = 1};
            }

            lexer.NextTokenOfKind(ETokenType.KwDo, out var lineOfDo, out _);
            var block = ParseBlock(lexer);

            lexer.NextTokenOfKind(ETokenType.KwEnd, out _, out _);
            return new ForNumStat
            {
                LineOfFor = lineOfToken,
                LineOfDo = lineOfDo,
                Block = block,
                InitExp = initExp,
                LimitExp = limitExp,
                StepExp = stepExp,
                VarName = name
            };
        }

        private static Stat ParseIfStat(Lexer.Lexer lexer)
        {
            var exps = new List<Exp>();
            var blocks = new List<Block>();

            lexer.NextTokenOfKind(ETokenType.KwIf, out _, out _);
            exps.Add(ParseExp(lexer));

            lexer.NextTokenOfKind(ETokenType.KwThen, out _, out _);
            blocks.Add(ParseBlock(lexer));

            while (lexer.LookAhead() == ETokenType.KwElseIf)
            {
                lexer.NextToken(out _, out _, out _);
                exps.Add(ParseExp(lexer));
                lexer.NextTokenOfKind(ETokenType.KwThen, out _, out _);
                blocks.Add(ParseBlock(lexer));
            }

            if (lexer.LookAhead() == ETokenType.KwElse)
            {
                lexer.NextToken(out _, out _, out _);
                exps.Add(new TrueExp {Line = lexer.Line});
                blocks.Add(ParseBlock(lexer));
            }

            lexer.NextTokenOfKind(ETokenType.KwEnd, out _, out _);
            return new IfStat {Exps = exps, Blocks = blocks};
        }

        private static Stat ParseRepeatStat(Lexer.Lexer lexer)
        {
            lexer.NextTokenOfKind(ETokenType.KwRepeat, out _, out _);
            var block = ParseBlock(lexer);
            lexer.NextTokenOfKind(ETokenType.KwUntil, out _, out _);
            var exp = ParseExp(lexer);
            return new RepeatStat {Block = block, Exp = exp};
        }

        private static Stat ParseWhileStat(Lexer.Lexer lexer)
        {
            lexer.NextTokenOfKind(ETokenType.KwWhile, out _, out _);
            var exp = ParseExp(lexer);
            lexer.NextTokenOfKind(ETokenType.KwDo, out _, out _);
            var block = ParseBlock(lexer);
            lexer.NextTokenOfKind(ETokenType.KwEnd, out _, out _);
            return new WhileStat {Exp = exp, Block = block};
        }

        private static Stat ParseDoStat(Lexer.Lexer lexer)
        {
            lexer.NextTokenOfKind(ETokenType.KwDo, out _, out _);
            var block = ParseBlock(lexer);
            lexer.NextTokenOfKind(ETokenType.KwEnd, out _, out _);
            return new DoStat {Block = block};
        }

        private static Stat ParseGotoStat(Lexer.Lexer lexer)
        {
            lexer.NextTokenOfKind(ETokenType.KwGoto, out _, out _);
            lexer.NextIdentifier(out _, out var name);
            return new GotoStat {Name = name};
        }

        private static Stat ParseLabelStat(Lexer.Lexer lexer)
        {
            lexer.NextTokenOfKind(ETokenType.SepLabel, out _, out _);
            lexer.NextIdentifier(out _, out var name);
            lexer.NextTokenOfKind(ETokenType.SepLabel, out _, out _);
            return new LabelStat {Name = name};
        }

        private static Stat ParseBreakStat(Lexer.Lexer lexer)
        {
            lexer.NextTokenOfKind(ETokenType.KwBreak, out _, out _);
            return new BreakStat {Line = lexer.Line};
        }

        private static Stat ParseEmptyStat(Lexer.Lexer lexer)
        {
            lexer.NextTokenOfKind(ETokenType.SepSemi, out _, out _);
            return new EmptyStat();
        }
    }
}