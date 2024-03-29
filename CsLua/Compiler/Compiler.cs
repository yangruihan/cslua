using CsLua.Binchunk;

namespace CsLua.Compiler
{
    internal static class Compiler
    {
        public static ProtoType Compile(string chunk, string chunkName)
        {
            var ast = Parser.Parser.Parse(chunk, chunkName);
            return CodeGen.CodeGen.GenProto(ast);
        }
    }
}