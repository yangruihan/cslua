namespace CsLua.Compiler.Ast
{
    class Block
    {
        public int LastLine;
        public Stat[] Stats;
        public Exp[] RetExps;
    }
}