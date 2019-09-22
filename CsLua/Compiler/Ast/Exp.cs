using System;
using System.Collections.Generic;
using System.Text;
using CsLua.Compiler.Lexer;
using CsLua.VM;

namespace CsLua.Compiler.Ast
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    class Exp : IAstNode
    {
        public virtual void Print(int offset)
        {
            for (var i = 0; i < offset; i++)
                Console.Write("\t");
        }
    }

    class NilExp : Exp
    {
        public int Line;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[NilExp Line:{Line}]");
        }
    }

    class TrueExp : Exp
    {
        public int Line;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[TrueExp Line:{Line}]");
        }
    }

    class FalseExp : Exp
    {
        public int Line;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[FalseExp Line:{Line}]");
        }
    }

    class VarargExp : Exp
    {
        public int Line;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[VarargExp Line:{Line}]");
        }
    }

    class IntegerExp : Exp
    {
        public int Line;
        public LuaInt Val;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[IntegerExp Line:{Line} Val:{Val}]");
        }
    }

    class FloatExp : Exp
    {
        public int Line;
        public LuaFloat Val;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[FloatExp Line:{Line} Val:{Val}]");
        }
    }

    class StringExp : Exp
    {
        public int Line;
        public string Str;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[StringExp Line:{Line} Str:{Str}]");
        }
    }

    class NameExp : Exp
    {
        public int Line;
        public string Name;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[NameExp Line:{Line} Name:{Name}]");
        }
    }

    class UnopExp : Exp
    {
        public int Line;
        public ETokenType Op;
        public Exp Exp;

        public EOpCode GetOpCode()
        {
            switch (Op)
            {
                case ETokenType.OpUnm:
                    return EOpCode.OP_UNM;

                case ETokenType.OpBNot:
                    return EOpCode.OP_BNOT;

                case ETokenType.OpLen:
                    return EOpCode.OP_LEN;

                case ETokenType.OpNot:
                    return EOpCode.OP_NOT;

                default:
                    return EOpCode.OP_UNKNOWN;
            }
        }

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[UnopExp Line:{Line} Op:{Op}]");
            Exp?.Print(offset + 1);
        }
    }

    class BinopExp : Exp
    {
        public int Line;
        public ETokenType Op;
        public Exp Exp1;
        public Exp Exp2;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[BinopExp Line:{Line} Op:{Op}]");
            Exp1?.Print(offset + 1);
            Exp2?.Print(offset + 1);
        }
    }

    class ConcatExp : Exp
    {
        public int Line;
        public List<Exp> Exps;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[ConcatExp Line:{Line}]");

            if (Exps != null)
                foreach (var exp in Exps)
                    exp?.Print(offset + 1);
        }
    }

    class TableConstructorExp : Exp
    {
        public int Line;
        public int LastLine;
        public List<Exp> KeyExps;
        public List<Exp> ValExps;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[TableConstructorExp Line:{Line} LastLine:{LastLine}]");

            if (KeyExps != null && ValExps != null)
            {
                for (var i = 0; i < KeyExps.Count; i++)
                {
                    var key = KeyExps[i];
                    var value = ValExps[i];
                    key?.Print(offset + 1);
                    value?.Print(offset + 1);
                }
            }
        }
    }

    class FuncDefExp : Exp
    {
        public int Line;
        public int LastLine;
        public List<string> ParList;
        public bool IsVararg;
        public Block Block;

        public override void Print(int offset)
        {
            base.Print(offset);
            var sb = new StringBuilder();

            foreach (var parName in ParList)
                sb.Append(parName).Append(" ");

            Console.WriteLine(
                $"[FuncDefExp Line:{Line} LastLine:{LastLine} IsVararg:{IsVararg} ParList:[{sb}]]");

            Block?.Print(offset + 1);
        }
    }

    class ParensExp : Exp
    {
        public Exp Exp;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[ParensExp]");
            Exp?.Print(offset + 1);
        }
    }

    class TableAccessExp : Exp
    {
        public int LastLine;
        public Exp PrefixExp;
        public Exp KeyExp;

        public override void Print(int offset)
        {
            base.Print(offset);
            Console.WriteLine($"[TableAccessExp LastLine:{LastLine}]");
            PrefixExp?.Print(offset + 1);
            KeyExp?.Print(offset + 1);
        }
    }

    class FuncCallExp : Exp
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
                $"[FuncCallExp Line:{Line} LastLine:{LastLine}]");

            PrefixExp?.Print(offset + 1);
            NameExp?.Print(offset + 1);

            if (Args != null)
                foreach (var arg in Args)
                    arg?.Print(offset + 1);
        }
    }
}