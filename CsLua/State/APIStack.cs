using CsLua.API;
using CsLua.Misc;

namespace CsLua.State
{
    /// <summary>
    /// 基础栈操作方法实现
    /// </summary>
    internal partial class LuaState : ILuaState
    {
        /// <summary>
        /// 返回栈顶索引
        /// </summary>
        public int GetTop()
        {
            return Top - (CallInfo.Func + 1);
        }

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
        /// 检查当前栈的容量是否够，不够则自动扩充
        /// </summary>
        public bool CheckStack(int n)
        {
            Stack.Check(n);
            return true;
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
        /// 将栈上 from 索引的值复制到 to 索引
        /// </summary>
        public void Copy(int fromIdx, int toIdx)
        {
            var val = Stack[fromIdx];
            Stack[toIdx] = val;
        }

        /// <summary>
        /// 把指定索引处的值推入栈顶
        /// </summary>
        public void PushValue(int idx)
        {
            var val = Stack[idx];
            Stack.Push(val);
        }

        /// <summary>
        /// 将栈顶值弹出，然后写入指定位置
        /// </summary>
        public void Replace(int idx)
        {
            var val = Stack.Pop();
            Stack[idx] = val;
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

        /// <summary>
        /// 将[idx, top]索引区间内的值朝栈顶方向旋转n个位置
        /// </summary>
        public void Rotate(int idx, int n)
        {
            var t = Top - 1;
            var p = Stack.AbsIndex(idx) - 1;
            var m = n >= 0 ? t - n : p - n - 1;
            Stack.Reverse(p, m);
            Stack.Reverse(m + 1, t);
            Stack.Reverse(p, t);
        }

        /// <summary>
        /// 将栈顶索引设置为指定值
        /// </summary>
        public void SetTop(int idx)
        {
            var func = CallInfo.Func;
            if (idx >= 0)
            {
                Check(idx <= Top - (func + 1), "new top too large");
            }
        }

        public void XMove(ILuaState to, int n)
        {
            if (to == this)
                return;

            var toState = to as LuaState;

            LuaAPI.CheckNElems(this, n);
            LuaAPI.Check(this, GlobalState == toState!.GlobalState,
                "moving among independent states");
            LuaAPI.Check(this, toState.Top >= n, "stack overflow");

            for (var i = 0; i < n; i++)
                toState.Stack.Push(this.Stack.Pop());
        }
    }
}