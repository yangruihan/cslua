using System.Collections.Generic;

namespace CsLua.Compiler.Lexer
{
    enum ETokenType
    {
        Unknown,
        Eof, // end of file
        Vararg, // ...
        SepSemi, // ;
        SepComma, // ,
        SepDot, // .
        SepColon, // :
        SepLabel, // ::
        SepLParen, // (
        SepRParen, // )
        SepLBrack, // [
        SepRBrack, // ]
        SepLCurly, // {
        SepRCurly, // }
        OpAssign, // =
        OpMinus, // -
        OpWave, // ~
        OpAdd, // +
        OpMul, // *
        OpDiv, // /
        OpIDiv, // //
        OpPow, // ^
        OpMod, // %
        OpBAnd, // &
        OpBOr, // |
        OpShr, // >>
        OpShl, // <<
        OpConcat, // ..
        OpLt, // <
        OpLe, // <=
        OpGt, // >
        OpGe, // >=
        OpEq, // ==
        OpNe, // ~=
        OpLen, // #
        OpAnd, // and
        OpOr, // or
        OpNot, // not
        KwBreak, // break
        KwDo, // do
        KwElse, // else
        KwElseIf, // elseif
        KwEnd, // end
        KwFalse, // false,
        KwFor, // for
        KwFunction, // function
        KwGoto, // goto
        KwIf, // if
        KwIn, // in
        KwLocal, // local
        KwNil, // nil
        KwRepeat, // repeat
        KwReturn, // return
        KwThen, // then
        KwTrue, // true
        KwUntil, // until
        KwWhile, // while
        Identifier, // identifier,
        Number, // number
        String, // string
        OpUnm = OpMinus,
        OpSub = OpMinus,
        OpBNot = OpWave,
        OpBXor = OpWave,
    }

    class Token
    {
        public static readonly Dictionary<string, ETokenType> Keywords = new Dictionary<string, ETokenType>
        {
            {"and", ETokenType.OpAnd},
            {"break", ETokenType.KwBreak},
            {"do", ETokenType.KwDo},
            {"else", ETokenType.KwElse},
            {"elseif", ETokenType.KwElseIf},
            {"end", ETokenType.KwEnd},
            {"false", ETokenType.KwFalse},
            {"for", ETokenType.KwFor},
            {"function", ETokenType.KwFunction},
            {"goto", ETokenType.KwGoto},
            {"if", ETokenType.KwIf},
            {"in", ETokenType.KwIn},
            {"local", ETokenType.KwLocal},
            {"nil", ETokenType.KwNil},
            {"not", ETokenType.OpNot},
            {"or", ETokenType.OpOr},
            {"repeat", ETokenType.KwRepeat},
            {"return", ETokenType.KwReturn},
            {"then", ETokenType.KwThen},
            {"true", ETokenType.KwTrue},
            {"until", ETokenType.KwUntil},
            {"while", ETokenType.KwWhile},
        };
    }
}