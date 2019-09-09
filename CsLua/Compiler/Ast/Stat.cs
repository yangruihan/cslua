namespace CsLua.Compiler.Ast
{
    class Stat
    {
    }

    class EmptyStat : Stat
    {
    }

    class BreakStat : Stat
    {
        public int Line;
    }

    class LabelStat : Stat
    {
        public string Name;
    }

    class GotoStat : Stat
    {
        public string Name;
    }

    class DoStat : Stat
    {
        public Block Block;
    }

    class FuncCallStat : Stat
    {
        public int Line;
        public int LastLine;
        public Exp PrefixExp;
        public NameExp StringExp;
        public Exp[] Args;
    }

    class WhileStat : Stat
    {
        public Exp Exp;
        public Block Block;
    }

    class RepeatStat : Stat
    {
        public Block Block;
        public Exp Exp;
    }

    class IfStat : Stat
    {
        public Exp[] Exps;
        public Block[] Blocks;
    }

    class ForStat : Stat
    {
        public int LineOfFor;
        public int LineOfDo;
        public string VarName;
        public Exp InitExp;
        public Exp LimitExp;
        public Exp StepExp;
        public Block Block;
    }

    class ForInStat : Stat
    {
        public int LineOfDo;
        public string[] NameList;
        public Exp[] ExpList;
        public Block Block;
    }

    class LocalVarDeclStat : Stat
    {
        public int LastLine;
        public string[] NameList;
        public Exp[] ExpList;
    }

    class AssignStat : Stat
    {
        public int LastLine;
        public Exp[] VarList;
        public Exp[] ExpList;
    }

    class LocalFuncDefStat : Stat
    {
        public string Name;
        public Exp FuncDefExp;
    }
}