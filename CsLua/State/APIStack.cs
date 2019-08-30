using CsLua.API;
using CsLua.Common;

namespace CsLua.State
{
    partial class LuaState : ILuaState
    {
        public int GetTop()
        {
            return _stack.Top;
        }

        public int AbsIndex(int idx)
        {
            return _stack.AbsIndex(idx);
        }

        public bool CheckStack(int n)
        {
            _stack.Check(n);
            return true;
        }

        public void Pop(int n)
        {
            for (var i = 0; i < n; i++)
                _stack.Pop();
        }

        public void Copy(int fromIdx, int toIdx)
        {
            var val = _stack[fromIdx];
            _stack[toIdx] = val;
        }

        public void PushValue(int idx)
        {
            var val = _stack[idx];
            _stack.Push(val);
        }

        public void Replace(int idx)
        {
            var val = _stack.Pop();
            _stack[idx] = val;
        }

        public void Insert(int idx)
        {
            Rotate(idx, 1);
        }

        public void Remove(int idx)
        {
            Rotate(idx, -1);
            Pop(1);
        }

        public void Rotate(int idx, int n)
        {
            var t = _stack.Top - 1;
            var p = _stack.AbsIndex(idx) - 1;
            var m = n >= 0 ? t - n : p - n - 1;
            _stack.Reverse(p, m);
            _stack.Reverse(m + 1, t);
            _stack.Reverse(p, t);
        }

        public void SetTop(int idx)
        {
            var newTop = _stack.AbsIndex(idx);
            if (newTop < 0)
                Debug.Panic("stack underflow!");

            var n = _stack.Top - newTop;
            if (n > 0)
            {
                for (var i = 0; i < n; i++)
                    _stack.Pop();
            }
            else
            {
                for (var i = 0; i > n; i--)
                    _stack.Push(LuaValue.Nil);
            }
        }
    }
}