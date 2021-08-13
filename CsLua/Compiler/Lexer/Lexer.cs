using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CsLua.Misc;

namespace CsLua.Compiler.Lexer
{
    internal class Lexer
    {
        #region Regex Attributes

        private static readonly Regex ReOpeningLongBracket = new Regex(@"^\[=*\[", RegexOptions.Compiled);
        private static readonly Regex ReNewLine = new Regex(@"\r\n|\n\r|\n|\r", RegexOptions.Compiled);

        private static readonly Regex ReShortStr =
            new Regex(@"(?s)(^'(\\\\|\\'|\\n|\\z\s*|[^'\n])*')|(^""(\\\\|\\""|\\n|\\z\s*|[^""\n])*"")");

        private static readonly Regex ReDecEscapeSeq = new Regex(@"^\\[0-9]{1,3}");
        private static readonly Regex ReHexEscapeSeq = new Regex(@"^\\x[0-9a-fA-F]{2}");
        private static readonly Regex ReUnicodeEscapeSeq = new Regex(@"^\\u\{[0-9a-fA-F]+\}");

        private static readonly Regex ReNumber =
            new Regex(@"^0[xX][0-9a-fA-F]*(\.[0-9a-fA-F]*)?([pP][+\-]?[0-9]+)?|^[0-9]*(\.[0-9]*)?([eE][+\-]?[0-9]+)?");

        private static readonly Regex ReIdentifier = new Regex(@"^[_\d\w]+");

        #endregion

        private string _chunk;
        private readonly string _chunkName;
        public int Line { private set; get; }

        private string _nextToken;
        private ETokenType _nextTokenKind;
        private int _nextTokenLine;

        public Lexer(string chunk, string chunkName)
        {
            _chunk = chunk;
            _chunkName = chunkName;
            Line = 1;
        }

        public void NextIdentifier(out int line, out string token)
        {
            NextTokenOfKind(ETokenType.Identifier, out line, out token);
        }

        public void NextTokenOfKind(ETokenType kind, out int line, out string token)
        {
            NextToken(out line, out var nextKind, out token);
            if (nextKind != kind)
                Error($"syntax error near '{token}'");
        }

        public ETokenType LookAhead()
        {
            if (_nextTokenLine > 0)
                return _nextTokenKind;

            var currentLine = Line;
            NextToken(out _nextTokenLine, out _nextTokenKind, out _nextToken);
            Line = currentLine;

            return _nextTokenKind;
        }

        public void NextToken(out int line, out ETokenType kind, out string token)
        {
            if (_nextTokenLine > 0)
            {
                Line = _nextTokenLine;
                kind = _nextTokenKind;
                line = _nextTokenLine;
                token = _nextToken;
                _nextTokenLine = 0;
                return;
            }

            SkipWhiteSpaces();
            if (_chunk.Length == 0)
            {
                line = Line;
                kind = ETokenType.Eof;
                token = "EOF";
                return;
            }

            switch (_chunk[0])
            {
                case ';':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.SepSemi;
                    token = ";";
                    return;
                }

                case ',':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.SepComma;
                    token = ",";
                    return;
                }

                case '(':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.SepLParen;
                    token = "(";
                    return;
                }

                case ')':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.SepRParen;
                    token = ")";
                    return;
                }

                case ']':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.SepRBracket;
                    token = "]";
                    return;
                }

                case '{':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.SepLCurly;
                    token = "{";
                    return;
                }

                case '}':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.SepRCurly;
                    token = "}";
                    return;
                }

                case '+':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.OpAdd;
                    token = "+";
                    return;
                }

                case '-':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.OpMinus;
                    token = "-";
                    return;
                }

                case '*':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.OpMul;
                    token = "*";
                    return;
                }

                case '^':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.OpPow;
                    token = "^";
                    return;
                }

                case '%':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.OpMod;
                    token = "%";
                    return;
                }

                case '&':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.OpBAnd;
                    token = "&";
                    return;
                }

                case '|':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.OpBOr;
                    token = "|";
                    return;
                }

                case '#':
                {
                    Next(1);
                    line = Line;
                    kind = ETokenType.OpLen;
                    token = "#";
                    return;
                }

                case ':':
                {
                    if (Test("::"))
                    {
                        Next(2);
                        line = Line;
                        kind = ETokenType.SepLabel;
                        token = "::";
                    }
                    else
                    {
                        Next(1);
                        line = Line;
                        kind = ETokenType.SepColon;
                        token = ":";
                    }

                    return;
                }

                case '/':
                {
                    if (Test("//"))
                    {
                        Next(2);
                        line = Line;
                        kind = ETokenType.OpIDiv;
                        token = "//";
                    }
                    else
                    {
                        Next(1);
                        line = Line;
                        kind = ETokenType.OpDiv;
                        token = "/";
                    }

                    return;
                }

                case '~':
                {
                    if (Test("~="))
                    {
                        Next(2);
                        line = Line;
                        kind = ETokenType.OpNe;
                        token = "~=";
                    }
                    else
                    {
                        Next(1);
                        line = Line;
                        kind = ETokenType.OpWave;
                        token = "~";
                    }

                    return;
                }

                case '=':
                {
                    if (Test("=="))
                    {
                        Next(2);
                        line = Line;
                        kind = ETokenType.OpEq;
                        token = "==";
                    }
                    else
                    {
                        Next(1);
                        line = Line;
                        kind = ETokenType.OpAssign;
                        token = "=";
                    }

                    return;
                }

                case '<':
                {
                    if (Test("<<"))
                    {
                        Next(2);
                        line = Line;
                        kind = ETokenType.OpShl;
                        token = "<<";
                    }
                    else if (Test("<="))
                    {
                        Next(2);
                        line = Line;
                        kind = ETokenType.OpLe;
                        token = "<=";
                    }
                    else
                    {
                        Next(1);
                        line = Line;
                        kind = ETokenType.OpLt;
                        token = "<";
                    }

                    return;
                }

                case '>':
                {
                    if (Test(">>"))
                    {
                        Next(2);
                        line = Line;
                        kind = ETokenType.OpShr;
                        token = ">>";
                    }
                    else if (Test(">="))
                    {
                        Next(2);
                        line = Line;
                        kind = ETokenType.OpGe;
                        token = ">=";
                    }
                    else
                    {
                        Next(1);
                        line = Line;
                        kind = ETokenType.OpGt;
                        token = ">";
                    }

                    return;
                }

                case '.':
                {
                    if (Test("..."))
                    {
                        Next(3);
                        line = Line;
                        kind = ETokenType.Vararg;
                        token = "...";

                        return;
                    }
                    else if (Test(".."))
                    {
                        Next(2);
                        line = Line;
                        kind = ETokenType.OpConcat;
                        token = "..";

                        return;
                    }
                    else if (_chunk.Length == 1 || !char.IsDigit(_chunk[1]))
                    {
                        Next(1);
                        line = Line;
                        kind = ETokenType.SepDot;
                        token = ".";

                        return;
                    }

                    break;
                }

                case '[':
                {
                    if (Test("[[") || Test("[="))
                    {
                        var t = ScanLongString();
                        line = Line;
                        kind = ETokenType.String;
                        token = t;
                    }
                    else
                    {
                        Next(1);
                        line = Line;
                        kind = ETokenType.SepLBracket;
                        token = "[";
                    }

                    return;
                }

                case '\'':
                case '"':
                {
                    var t = ScanShortString();
                    line = Line;
                    kind = ETokenType.String;
                    token = t;

                    return;
                }
            }

            var c = _chunk[0];

            // number
            if (c == '.' || char.IsDigit(c))
            {
                token = ScanNumber();
                line = Line;
                kind = ETokenType.Number;
                return;
            }

            if (c == '_' || char.IsLetter(c))
            {
                token = ScanIdentifier();
                line = Line;

                if (Token.Keywords.ContainsKey(token))
                {
                    kind = Token.Keywords[token];
                    return;
                }
                else
                {
                    kind = ETokenType.Identifier;
                    return;
                }
            }

            line = 0;
            kind = ETokenType.Unknown;
            token = "";

            Error($"unexpected symbol near {c}");
        }

        private string Scan(Regex re)
        {
            var ret = re.Match(_chunk);
            if (ret.Success)
            {
                Next(ret.Value.Length);
                return ret.Value;
            }

            Debug.Panic("unreachable!");
            return string.Empty;
        }

        private string ScanIdentifier()
        {
            return Scan(ReIdentifier);
        }

        private string ScanNumber()
        {
            return Scan(ReNumber);
        }

        private string ScanShortString()
        {
            var shortStr = ReShortStr.Match(_chunk);
            if (shortStr.Success)
            {
                var str = shortStr.Value;
                Next(str.Length);
                str = str.Substring(1, str.Length - 2);
                if (str.IndexOf("\\", StringComparison.Ordinal) >= 0)
                {
                    var newLineRet = ReNewLine.Matches(str);
                    Line += newLineRet.Count;
                    str = Escape(str);
                }

                return str;
            }

            Error("unfinished string");
            return string.Empty;
        }

        private string ScanLongString()
        {
            var openingLongBracket = ReOpeningLongBracket.Match(_chunk);
            if (!openingLongBracket.Success)
            {
                Error("Invalid long string delimiter near '%s'", _chunk.Substring(0, 2));
            }

            var closingLongBracket = openingLongBracket.Value.Replace("[", "]");
            var closingLongBracketIdx = _chunk.IndexOf(closingLongBracket, StringComparison.Ordinal);

            if (closingLongBracketIdx < 0)
            {
                Error("unfinished long string or comment");
            }

            var str = _chunk.Substring(openingLongBracket.Length,
                closingLongBracketIdx - openingLongBracket.Length);
            Next(closingLongBracketIdx + closingLongBracket.Length);
            str = ReNewLine.Replace(str, "\n");
            Line += str.Count(c => c == '\n');
            if (str.Length > 0 && str[0] == '\n')
                str = str.Substring(1);

            return str;
        }

        private string Escape(string str)
        {
            var sb = new StringBuilder();
            while (str.Length > 0)
            {
                if (str[0] != '\\')
                {
                    sb.Append(str[0]);
                    str = str.Substring(1);
                    continue;
                }

                if (str.Length == 1)
                {
                    Error("unfinished string");
                    return "";
                }

                switch (str[1])
                {
                    case 'a':
                        sb.Append('\a');
                        str = str.Substring(2);
                        continue;

                    case 'b':
                        sb.Append('\b');
                        str = str.Substring(2);
                        continue;

                    case 'f':
                        sb.Append('\f');
                        str = str.Substring(2);
                        continue;

                    case 'n':
                        sb.Append('\n');
                        str = str.Substring(2);
                        continue;

                    case '\n':
                        sb.Append('\n');
                        str = str.Substring(2);
                        continue;

                    case 'r':
                        sb.Append('\r');
                        str = str.Substring(2);
                        continue;

                    case 't':
                        sb.Append('\t');
                        str = str.Substring(2);
                        continue;

                    case 'v':
                        sb.Append('\v');
                        str = str.Substring(2);
                        continue;

                    case '"':
                        sb.Append('\"');
                        str = str.Substring(2);
                        continue;

                    case '\'':
                        sb.Append('\'');
                        str = str.Substring(2);
                        continue;

                    case '\\':
                        sb.Append('\\');
                        str = str.Substring(2);
                        continue;

                    // \ddd
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    {
                        var decEscapeRet = ReDecEscapeSeq.Match(str);
                        if (decEscapeRet.Success)
                        {
                            if (Int32.TryParse(decEscapeRet.Value.Substring(1), out var ret))
                            {
                                if (ret < 0xff)
                                {
                                    sb.Append((char) ret);
                                    str = str.Substring(decEscapeRet.Value.Length);
                                    continue;
                                }
                            }

                            Error($"decimal escape too large near '{decEscapeRet.Value}'");
                        }

                        break;
                    }

                    // \xXX
                    case 'x':
                    {
                        var hexEscapeRet = ReHexEscapeSeq.Match(str);
                        if (hexEscapeRet.Success)
                        {
                            if (Int32.TryParse(hexEscapeRet.Value.Substring(2), NumberStyles.HexNumber,
                                new NumberFormatInfo(), out var ret))
                            {
                                sb.Append((char) ret);
                                str = str.Substring(hexEscapeRet.Length);
                                continue;
                            }
                        }

                        break;
                    }

                    // \u{XXX}
                    case 'u':
                    {
                        var unicodeEscapeRet = ReUnicodeEscapeSeq.Match(str);
                        if (unicodeEscapeRet.Success)
                        {
                            if (Int32.TryParse(unicodeEscapeRet.Value.Substring(3, unicodeEscapeRet.Value.Length - 4),
                                NumberStyles.HexNumber, new NumberFormatInfo(), out var ret))
                            {
                                if (ret < 0x10ffff)
                                {
                                    sb.Append((char) ret);
                                    str = str.Substring(unicodeEscapeRet.Length);
                                    continue;
                                }
                            }

                            Error($"UTF-8 value too large near '{unicodeEscapeRet.Value}'");
                        }

                        break;
                    }

                    case 'z':
                    {
                        str = str.Substring(2);
                        if (str.Length > 0 && IsWhiteSpace(str[0]))
                            str = str.Substring(1);

                        continue;
                    }
                }

                Error($"invalid escape sequence near '\\{str[1]}'");
            }

            return sb.ToString();
        }

        private void SkipWhiteSpaces()
        {
            while (_chunk.Length > 0)
            {
                if (Test("--"))
                {
                    SkipComment();
                }
                else if (Test("\r\n") || Test("\n\r"))
                {
                    Next(2);
                    Line++;
                }
                else if (IsNewLine(_chunk[0]))
                {
                    Next(1);
                    Line++;
                }
                else if (IsWhiteSpace(_chunk[0]))
                {
                    Next(1);
                }
                else
                {
                    break;
                }
            }
        }

        private void SkipComment()
        {
            // skip --
            Next(2);

            // long comment
            if (Test("["))
            {
                if (ReOpeningLongBracket.IsMatch(_chunk))
                {
                    ScanLongString();
                    return;
                }
            }

            // short comment
            while (_chunk.Length > 0 && !IsNewLine(_chunk[0]))
                Next(1);
        }

        private bool IsNewLine(char c)
        {
            return c == '\r' || c == '\n';
        }

        private bool IsWhiteSpace(char c)
        {
            return c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r' || c == ' ';
        }

        private void Next(int n)
        {
            _chunk = _chunk.Substring(n);
        }

        private bool Test(string s)
        {
            return _chunk.StartsWith(s);
        }

        private void Error(string f, params object[] parameters)
        {
            var err = string.Format(f, parameters);
            err = $"{_chunkName}:{Line} {err}";
            Debug.Panic(err);
        }
    }
}