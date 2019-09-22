using System;
using System.Collections.Generic;
using System.Text;

namespace CsLua.Compiler.Ast
{
    interface IAstNode
    {
        void Print(int offset);
    }

    class Block : IAstNode
    {
        public int LastLine;
        public List<Stat> Stats;
        public List<Exp> RetExps;

        public void Print(int offset)
        {
            for (var i = 0; i < offset; i++)
                Console.Write("\t");

            Console.WriteLine($"[Block LastLine:{LastLine}]");

            if (Stats != null)
            {
                for (var i = 0; i < offset + 1; i++)
                    Console.Write("\t");
                Console.WriteLine("----- Stats -----");

                foreach (var stat in Stats)
                    stat.Print(offset + 1);
            }

            if (RetExps != null)
            {
                for (var i = 0; i < offset + 1; i++)
                    Console.Write("\t");
                Console.WriteLine("----- Return Exps -----");
                
                foreach (var retExp in RetExps)
                    retExp.Print(offset + 1);
            }
        }
    }
}