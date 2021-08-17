using CsLua.API;

namespace CsLua.State
{
    /// <summary>
    /// 基础栈操作方法实现
    /// </summary>
    internal partial class LuaState : ILuaState
    {
        /// <summary>
        /// 把索引转换为绝对索引
        /// </summary>
        public int AbsIndex(int idx)
        {
            return idx > 0 || LuaAPI.IsPseudo(idx)
                ? idx
                : Top - CallInfo.Func + idx;
        }

        /// <summary>
        /// 返回栈顶索引
        /// </summary>
        public int GetTop()
        {
            return Top - (CallInfo.Func + 1);
        }

        /// <summary>
        /// 将栈顶索引设置为指定值
        /// </summary>
        public void SetTop(int idx)
        {
            var func = CallInfo.Func;
            if (idx >= 0)
            {
                Check(idx <= StackLast - (func + 1), "new top too large");
                while (Top < (func + 1) + idx)
                    PushNil();
            }
            else
            {
                Check(-(idx + 1) <= (Top - (func + 1)), "invalid new top");
                Stack.PopN(idx + 1);
            }
        }

        /// <summary>
        /// 把指定索引处的值推入栈顶
        /// </summary>
        public void PushValue(int idx)
        {
            Push(GetValueByRelIdx(idx));
        }

        /// <summary>
        /// 将[idx, top]索引区间内的值朝栈顶方向旋转n个位置
        /// </summary>
        public void Rotate(int idx, int n)
        {
            var t = Top - 1; // end of stack segment being rotated
            var val = GetValueByRelIdx(idx, out var p)!; // start of segment
            CheckStackIndex(idx, val);
            Check((n >= 0 ? n : -n) <= (t - p + 1), "invalid 'n'");
            var m = n >= 0 ? t - n : p - n - 1; // end of prefix
            Stack.Reverse(p, m); // reverse the prefix with length 'n'
            Stack.Reverse(m + 1, t); // reverse the suffix
            Stack.Reverse(p, t); // reverse the entire segment
        }

        /// <summary>
        /// 将栈上 from 索引的值复制到 to 索引
        /// </summary>
        public void Copy(int fromIdx, int toIdx)
        {
            var frVal = GetValueByRelIdx(fromIdx, out var fr)!;
            var toVal = GetValueByRelIdx(toIdx, out var to)!;
            CheckValidIndex(toVal);
            Stack[to] = frVal;

            if (IsUpvalue(toIdx)) // function upvalue?
            {
                // TODO
                // LUA_REGISTRYINDEX does not need gc barrier (collector revisits it before finishing collection)
            }
        }

        /// <summary>
        /// 检查当前栈的容量是否够，不够则自动扩充
        /// </summary>
        public bool CheckStack(int n)
        {
            var ci = CallInfo;
            Check(n >= 0, "negative 'n'");
            if (Stack.Check(n))
            {
                return true;
            }
            else
            {
                int inuse = Top + LuaConst.EXTRA_STACK;
                // can grow without overflow?
                if (inuse > LuaConst.LUAI_MAXSTACK - n)
                    return false;
                else
                {
                    if (ci.Top < Top + n)
                    {
                        // adjust frame top
                        ci.Top = Top + n;
                    }

                    return true;
                }
            }
        }

        /// <summary>
        /// 将 n 个元素从一个栈移动到另一个栈中
        /// </summary>
        public void XMove(ILuaState to, int n)
        {
            if (to == this)
                return;

            var toState = to as LuaState;

            CheckNElems(n);
            Check(GlobalState == toState!.GlobalState,
                "moving among independent states");
            Check(toState.Top >= n, "stack overflow");

            for (var i = 0; i < n; i++)
                toState.Stack.Push(this.Stack.Pop());
        }

        /// <summary>
        /// 从栈顶弹出 n 个值
        /// </summary>
        public void Pop(int n)
        {
            for (var i = 0; i < n; i++)
                Stack.Pop();
        }

        /// <summary>
        /// 将栈顶值弹出，然后写入指定位置
        /// </summary>
        public void Replace(int idx)
        {
            var val = Stack.Pop();
            SetValue(idx, val);
        }

        /// <summary>
        /// 将栈顶值弹出，然后插入指定位置
        /// </summary>
        public void Insert(int idx)
        {
            Rotate(idx, 1);
        }

        /// <summary>
        /// 删除指定索引处的值，然后将该值上面的值全部下移一个位置
        /// </summary>
        public void Remove(int idx)
        {
            Rotate(idx, -1);
            Pop(1);
        }
    }
}