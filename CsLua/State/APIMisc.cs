using CsLua.API;
using CsLua.Common;

namespace CsLua.State
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    /// <summary>
    /// 其他辅助操作运算实现
    /// </summary>
    partial class LuaState : ILuaState
    {
        /// <summary>
        /// 访问指定索引处的值，取其长度，然后推入栈顶
        /// </summary>
        /// <param name="idx"></param>
        public void Len(int idx)
        {
            var val = _stack[idx];
            if (val.Value is string s)
            {
                _stack.Push((LuaInt) s.Length);
            }
            else if (LuaValue.CallMetaMethod(val, val, "__len", this, out var metaMethodRet))
            {
                _stack.Push(metaMethodRet);
            }
            else if (val.Value is LuaTable t)
            {
                _stack.Push((LuaInt) t.Len());
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

                    var b = _stack.Pop();
                    var a = _stack.Pop();
                    if (LuaValue.CallMetaMethod(a, b, "__concat", this, out var metaMethodRet))
                    {
                        _stack.Push(metaMethodRet);
                        continue;
                    }

                    Debug.Panic("concatenation error!");
                }
            }
        }

        public bool Next(int idx)
        {
            var val = _stack[idx];
            if (val.Value is LuaTable lt)
            {
                var key = _stack.Pop();
                var nextKey = lt.NextKey(key);
                if (nextKey.Value != null)
                {
                    _stack.Push(nextKey);
                    _stack.Push(lt.Get(nextKey));
                    return true;
                }

                return false;
            }

            Debug.Panic("table expected!");
            return false;
        }

        public int Error()
        {
            var err = _stack.Pop();
            Debug.Panic(err.ToString());
            return -1;
        }
    }
}