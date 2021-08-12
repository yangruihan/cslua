using CsLua.Compiler.Ast;
using CsLua.Compiler.Lexer;

namespace CsLua.Compiler.Parser
{
    internal static partial class Parser
    {
        public static Block Parse(string chunk, string chunkName)
        {
            var lexer = new Lexer.Lexer(chunk, chunkName);
            var block = ParseBlock(lexer);
            lexer.NextTokenOfKind(ETokenType.Eof, out _, out _);
            return block;
        }
    }
}