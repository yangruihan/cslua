using CsLua.API;
using CsLua.Misc;

namespace CsLua.State
{
    /// <summary>
    /// 其他辅助操作运算实现
    /// </summary>
    internal partial class LuaState : ILuaState
    {
        /// <summary>
        /// 访问指定索引处的值，取其长度，然后推入栈顶
        /// </summary>
        /// <param name="idx"></param>
        public void Len(int idx)
        {
            var val = Stack[idx];
            if (val.IsString())
            {
                Stack.Push((LuaInt) val.GetStrValue().Length);
            }
            else if (LuaValue.CallMetaMethod(val, val, "__len", this,
                out var metaMethodRet))
            {
                Stack.Push(metaMethodRet);
            }
            else if (val.IsTable())
            {
                Stack.Push((LuaInt) val.GetTableValue().Len());
            }
            else
            {
                Debug.Panic("length error!");
            }
        }

        /// <summary>
        /// 执行字符串拼接运算
        /// </summary>
        public void Concat(int n)
        {
            if (n == 0)
            {
                Stack.Push("");
            }
            else if (n >= 2)
            {
                for (var i = 1; i < n; i++)
                {
                    if (IsString(-1) && IsString(-2))
                    {
                        var s2 = ToString(-1);
                        var s1 = ToString(-2);
                        Stack.Pop();
                        Stack.Pop();
                        Stack.Push(s1 + s2);
                        continue;
                    }

                    var b = Stack.Pop();
                    var a = Stack.Pop();
                    if (LuaValue.CallMetaMethod(a, b, "__concat", this,
                        out var metaMethodRet))
                    {
                        Stack.Push(metaMethodRet);
                        continue;
                    }

                    Debug.Panic("concatenation error!");
                }
            }
        }

        public bool Next(int idx)
        {
            var val = Stack[idx];
            if (val.IsTable())
            {
                var lt = val.GetTableValue();
                var key = Stack.Pop();
                var nextKey = lt.NextKey(key);
                if (!nextKey.IsNil())
                {
                    Stack.Push(nextKey);
                    Stack.Push(lt.Get(nextKey));
                    return true;
                }

                return false;
            }

            Debug.Panic("table expected!");
            return false;
        }

        public int Error()
        {
            var err = Stack.Pop();
            Debug.Panic(err.ToString());
            return -1;
        }

        public void Assert(bool cond)
        {
            System.Diagnostics.Debug.Assert(cond);
        }
    }
}