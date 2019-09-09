using System;

namespace CsLua.Compiler.Ast
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    class Exp
    {
    }

    class NilExp : Exp
    {
        public int Line;
    }

    class TrueExp : Exp
    {
        public int Line;
    }

    class FalseExp : Exp
    {
        public int Line;
    }

    class VarargExp : Exp
    {
        public int Line;
    }

    class IntegerExp : Exp
    {
        public int Line;
        public LuaInt Val;
    }

    class FloatExp : Exp
    {
        public int Line;
        public LuaFloat Val;
    }

    class StringExp : Exp
    {
        public int Line;
        public string Str;
    }

    class NameExp : Exp
    {
        public int Line;
        public string Name;
    }

    class UnopExp : Exp
    {
        public int Line;
        public int Op;
        public Exp Exp;
    }

    class BinopExp : Exp
    {
        public int Line;
        public int Op;
        public Exp Exp1;
        public Exp Exp2;
    }

    class ConcatExp : Exp
    {
        public int Line;
        public Exp[] Exps;
    }

    class TableConstructorExp : Exp
    {
        public int Line;
        public int LastLine;
        public Exp[] KeyExps;
        public Exp[] ValExps;
    }

    class FuncDefExp : Exp
    {
        public int Line;
        public int LastLine;
        public string[] ParList;
        public bool IsVararg;
        public Block Block;
    }

    class ParensExp : Exp
    {
        private Exp Exp;
    }

    class TableAccessExp : Exp
    {
        public int LastLine;
        public Exp PrefixExp;
        public Exp KeyExp;
    }

    class FuncCallExp : Exp
    {
        public int Line;
        public int LastLine;
        public Exp PrefixExp;
        public NameExp StringExp;
        public Exp[] Args;
    }
}