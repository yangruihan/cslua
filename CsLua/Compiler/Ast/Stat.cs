using System;
using System.Collections.Generic;
using System.Text;

namespace CsLua.Compiler.Ast
{
    class Stat : IAstNode
    {
        public virtual void Print(int offset)
        {
            for (var i = 0; i < offset; i++)
                Console.Write("\t");
        }
    }

    class EmptyStat : Stat
    {
        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[EmptyStat]");
        }
    }

    class BreakStat : Stat
    {
        public int Line;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[BreakStat Line:{Line}]");
        }
    }

    class LabelStat : Stat
    {
        public string Name;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[BreakStat Name:{Name}]");
        }
    }

    class GotoStat : Stat
    {
        public string Name;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[GotoStat Name:{Name}]");
        }
    }

    class DoStat : Stat
    {
        public Block Block;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[DoStat]");
            Block?.Print(offset + 1);
        }
    }

    class FuncCallStat : Stat
    {
        public int Line;
        public int LastLine;
        public Exp PrefixExp;
        public StringExp NameExp;
        public List<Exp> Args;

        public override void Print(int offset)
        {
            base.Print(offset);

            Console.WriteLine(
                $"[FuncCallStat Line:{Line} LastLine:{LastLine}]");

            PrefixExp?.Print(offset + 1);
            NameExp?.Print(offset + 1);
            
            if (Args != null)
                foreach (var arg in Args)
                    arg?.Print(offset + 1);
        }

        public static implicit operator FuncCallStat(FuncCallExp exp)
        {
            return new FuncCallStat
            {
                Line = exp.Line,
                LastLine = exp.LastLine,
                PrefixExp = exp.PrefixExp,
                NameExp = exp.NameExp,
                Args = exp.Args
            };
        }

        public static implicit operator FuncCallExp(FuncCallStat stat)
        {
            return new FuncCallExp
            {
                Line = stat.Line,
                LastLine = stat.LastLine,
                PrefixExp = stat.PrefixExp,
                NameExp = stat.NameExp,
                Args = stat.Args
            };
        }
    }

    class WhileStat : Stat
    {
        public Exp Exp;
        public Block Block;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[WhileStat]");
            Exp?.Print(offset + 1);
            Block?.Print(offset + 1);
        }
    }

    class RepeatStat : Stat
    {
        public Block Block;
        public Exp Exp;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[RepeatStat]");
            Block?.Print(offset + 1);
            Exp?.Print(offset + 1);
        }
    }

    class IfStat : Stat
    {
        public List<Exp> Exps;
        public List<Block> Blocks;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[IfStat]");
            
            if (Exps != null)
                foreach (var exp in Exps)
                    exp?.Print(offset + 1);

            if (Blocks != null)
                foreach (var block in Blocks)
                    block?.Print(offset + 1);
        }
    }

    class ForNumStat : Stat
    {
        public int LineOfFor;
        public int LineOfDo;
        public string VarName;
        public Exp InitExp;
        public Exp LimitExp;
        public Exp StepExp;
        public Block Block;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[ForStat LineOfFor:{LineOfFor} LineOfDo:{LineOfDo} VarName:{VarName}]");
            InitExp?.Print(offset + 1);
            LimitExp?.Print(offset + 1);
            StepExp?.Print(offset + 1);
            Block?.Print(offset + 1);
        }
    }

    class ForInStat : Stat
    {
        public int LineOfDo;
        public List<string> NameList;
        public List<Exp> ExpList;
        public Block Block;

        public override void Print(int offset)
        {
            base.Print(offset);
            var sb = new StringBuilder();
            if (NameList != null)
                foreach (var name in NameList)
                    sb.Append(name).Append(" ");

            Console.WriteLine(
                $"[ForInStat LineOfDo:{LineOfDo} NameList:[{sb}]]");

            if (ExpList != null)
                foreach (var exp in ExpList)
                    exp?.Print(offset + 1);

            Block?.Print(offset + 1);
        }
    }

    class LocalVarDeclStat : Stat
    {
        public int LastLine;
        public List<string> NameList;
        public List<Exp> ExpList;

        public override void Print(int offset)
        {
            base.Print(offset);
            var sb = new StringBuilder();
            
            if (NameList != null)
                foreach (var name in NameList)
                    sb.Append(name).Append(" ");

            Console.WriteLine(
                $"[LocalVarDeclStat LastLine:{LastLine} NameList:[{sb}]]");

            if (ExpList != null)
                foreach (var exp in ExpList)
                    exp?.Print(offset + 1);
        }
    }

    class AssignStat : Stat
    {
        public int LastLine;
        public List<Exp> VarList;
        public List<Exp> ExpList;

        public override void Print(int offset)
        {
            base.Print(offset);

            Console.WriteLine(
                $"[AssignStat LastLine:{LastLine}]");

            if (VarList != null)
                foreach (var exp in VarList)
                    exp?.Print(offset + 1);

            if (ExpList != null)
                foreach (var exp in ExpList)
                    exp?.Print(offset + 1);
        }
    }

    class LocalFuncDefStat : Stat
    {
        public string Name;
        public FuncDefExp Exp;

        public override void Print(int offset)
        {
            base.Print(offset);

            Console.WriteLine(
                $"[LocalFuncDefStat Name:{Name}]");

            Exp?.Print(offset + 1);
        }
    }
}