using System.Collections.Generic;
using System.Text;
using CsLua.API;
using CsLua.Misc;

namespace CsLua.State
{
    internal class LuaStack
    {
        private readonly List<LuaValue?> _slots;
        public List<LuaValue?> Slots => _slots;

        public int Top;

        public LuaStack(int size)
        {
            _slots = new List<LuaValue?>(size);
            for (var i = 0; i < size; i++)
                _slots.Add(null);
            Top = 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"*Stack* size: {_slots.Count} top: {Top} items: [");
            for (var i = 0; i < Slots.Count - 1; i++)
                sb.Append($"{i + 1}: {Slots[i]}, ");
            sb.Append($"{Slots.Count}: {Slots[^1]}]");
            return sb.ToString();
        }

        public LuaValue? this[int i]
        {
            get => Get(i);
            set => Set(i, value);
        }

        /// <summary>
        /// check stack size and grow stack if needed
        /// </summary>
        public bool Check(int n)
        {
            var free = _slots.Capacity - Top;
            for (var i = free; i < n; i++)
                _slots.Add(null);

            return free >= n;
        }

        public void Push(bool b)
        {
            Push(b ? LuaValue.True : LuaValue.False);
        }

        public void Push(LuaFloat f)
        {
            Push(LuaValue.Create(f));
        }

        public void Push(LuaInt i)
        {
            Push(LuaValue.Create(i));
        }

        public void Push(string str)
        {
            Push(LuaValue.Create(str));
        }

        public void PushNil()
        {
            Push(LuaValue.Nil);
        }

        public void Push(object o)
        {
            Push(LuaValue.Create(o));
        }

        public void Push(LuaValue? value)
        {
            if (Top == _slots.Capacity)
                Debug.Panic("stack overflow!");

            _slots[Top++] = value;
        }

        public LuaValue? Pop()
        {
            if (Top < 1)
                Debug.Panic("stack underflow!");

            var val = _slots[--Top];
            _slots[Top] = null;
            return val;
        }

        public void PushN(LuaValue[]? vals, int n)
        {
            if (vals == null)
                return;

            var nVals = vals.Length;
            if (n < 0)
                n = nVals;

            for (var i = 0; i < n; i++)
            {
                Push(i < nVals ? vals[i] : LuaValue.Nil);
            }
        }

        public void PopN(int n)
        {
            for (var i = n - 1; i >= 0; i--)
            {
                Pop();
            }
        }

        public void PopN(int n, out LuaValue?[] ret)
        {
            ret = new LuaValue?[n];
            for (var i = n - 1; i >= 0; i--)
            {
                ret[i] = Pop();
            }
        }

        public void Reverse(int from, int to)
        {
            var slots = _slots;
            while (from < to)
            {
                (slots[@from], slots[to]) = (slots[to], slots[@from]);
                from++;
                to--;
            }
        }

        private LuaValue? Get(int idx)
        {
            if (idx >= 0 && idx < Top)
                return _slots[idx];

            return null;
        }

        private void Set(int idx, LuaValue? val)
        {
            if (idx > 0 && idx <= Top)
            {
                _slots[idx - 1] = val;
                return;
            }

            Debug.Panic("invalid index!");
        }
    }
}