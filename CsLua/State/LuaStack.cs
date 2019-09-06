using System.Collections.Generic;
using CsLua.API;
using CsLua.Common;

namespace CsLua.State
{
    class LuaStack
    {
        private readonly List<LuaValue> _slots;
        public int Top;

        public LuaState State;
        public Closure Closure;
        public LuaValue[] Varargs;
        
        public int PC;
        public LuaStack Prev;

        public LuaStack(int size, LuaState state)
        {
            _slots = new List<LuaValue>(size);
            for (var i = 0; i < size; i++)
                _slots.Add(null);
            Top = 0;
            State = state;
        }

        public LuaValue this[int i]
        {
            get => Get(i);
            set => Set(i, value);
        }

        public void Check(int n)
        {
            var free = _slots.Capacity - Top;
            for (var i = free; i < n; i++)
                _slots.Add(LuaValue.Nil);
        }

        public void Push(bool b)
        {
            Push(b ? LuaValue.True : LuaValue.False);
        }

        public void Push(object o)
        {
            if (o is null)
                Push(LuaValue.Nil);
            else
                Push(new LuaValue(o));
        }

        public void Push(LuaValue value)
        {
            if (Top == _slots.Capacity)
                Debug.Panic("stack overflow!");

            _slots[Top++] = value;
        }

        public LuaValue Pop()
        {
            if (Top < 1)
                Debug.Panic("stack underflow!");

            var val = _slots[--Top];
            _slots[Top] = null;
            return val;
        }

        public void PushN(LuaValue[] vals, int n)
        {
            var nVals = vals.Length;
            if (n < 0)
                n = nVals;

            for (var i = 0; i < n; i++)
            {
                Push(i < nVals ? vals[i] : LuaValue.Nil);
            }
        }

        public LuaValue[] PopN(int n)
        {
            var vals = new LuaValue[n];
            for (var i = n - 1; i >= 0; i--)
            {
                vals[i] = Pop();
            }

            return vals;
        }

        public int AbsIndex(int idx)
        {
            if (idx <= Consts.LUA_REGISTRYINDEX)
                return idx;
            
            if (idx > 0)
                return idx;

            return idx + Top + 1;
        }

        public bool IsValid(int idx)
        {
            if (idx == Consts.LUA_REGISTRYINDEX)
                return true;
            
            var absIdx = AbsIndex(idx);
            return absIdx > 0 && absIdx <= Top;
        }

        public LuaValue Get(int idx)
        {
            if (idx == Consts.LUA_REGISTRYINDEX)
                return State.GetRegistry();
            
            var absIndex = AbsIndex(idx);
            if (absIndex > 0 && absIndex <= Top)
                return _slots[absIndex - 1];
            return null;
        }

        public void Set(int idx, LuaValue val)
        {
            if (idx == Consts.LUA_REGISTRYINDEX)
            {
                State.SetRegistry(val.Value as LuaTable);
                return;
            }
            
            var absIndex = AbsIndex(idx);
            if (absIndex > 0 && absIndex <= Top)
            {
                _slots[absIndex - 1] = val;
                return;
            }

            Debug.Panic("invalid index!");
        }

        public void Reverse(int from, int to)
        {
            var slots = _slots;
            while (from < to)
            {
                var temp = slots[from];
                slots[from] = slots[to];
                slots[to] = temp;
                from++;
                to--;
            }
        }
    }
}