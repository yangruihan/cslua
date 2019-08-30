using CsLua.API;
using CsLua.Common;

namespace CsLua.State
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    partial class LuaState : ILuaState
    {
        public void Len(int idx)
        {
            var val = _stack[idx];
            if (val.Value is string)
            {
                _stack.Push((LuaInt) ((string) val.Value).Length);
            }
            else
            {
                Debug.Panic("length error!");
            }
        }

        public void Concat(int n)
        {
            if (n == 0)
            {
                _stack.Push("");
            }
            else if (n >= 2)
            {
                for (var i = 1; i < n; i++)
                {
                    if (IsString(-1) && IsString(-2))
                    {
                        var s2 = ToString(-1);
                        var s1 = ToString(-2);
                        _stack.Pop();
                        _stack.Pop();
                        _stack.Push(s1 + s2);
                        continue;
                    }

                    Debug.Panic("concatenation error!");
                }
            }
        }
    }
}