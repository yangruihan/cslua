using System.Collections.Generic;
using CsLua.Common;
using CsLua.Number;

namespace CsLua.State
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    class LuaTable
    {
        private List<LuaValue> _arr;
        private Dictionary<object, LuaValue> _map;
        public LuaTable MetaTable;

        public LuaTable(int nArr, int nRec)
        {
            if (nArr > 0)
            {
                _arr = new List<LuaValue>(nArr);
            }

            if (nRec > 0)
            {
                _map = new Dictionary<object, LuaValue>(nRec);
            }
        }

        public int Len()
        {
            return _arr?.Count ?? 0;
        }

        public LuaValue Get(object key)
        {
            return Get(new LuaValue(key));
        }

        public LuaValue Get(LuaValue key)
        {
            LuaInt idx = 0;
            if (key.Value is LuaInt i)
                idx = i;
            else if (key.Value is LuaFloat f)
                if (LuaMath.FloatToInteger(f, out var fi))
                    idx = fi;

            if (idx >= 1 && idx <= Len())
                return _arr[(int) (idx - 1)];

            return _map != null && _map.TryGetValue(key.Value, out var ret) ? ret : null;
        }

        public void Put(object k, object v)
        {
            LuaValue key;
            LuaValue value;
            if (k is LuaValue kl)
                key = kl;
            else
                key = new LuaValue(k);

            if (v is LuaValue vl)
                value = vl;
            else
                value = new LuaValue(v);

            Put(key, value);
        }

        public void Put(LuaValue key, LuaValue val)
        {
            if (key is null || key.Value is null)
                Debug.Panic("table index is nil!");

            if (key.Value is LuaFloat f && LuaFloat.IsNaN(f))
                Debug.Panic("table index is Nan!");

            LuaInt idx = 0;
            if (key.Value is LuaInt i)
                idx = i;
            else if (key.Value is LuaFloat vf)
                if (LuaMath.FloatToInteger(vf, out var fi))
                    idx = fi;

            if (idx >= 1)
            {
                var arrLen = Len();
                if (idx <= arrLen)
                {
                    _arr[(int) (idx - 1)] = val;
                    if (idx == arrLen && val is null)
                        ShrinkArray();
                    return;
                }

                if (idx == arrLen + 1)
                {
                    if (_map != null && _map.ContainsKey(key.Value))
                        _map.Remove(key.Value);

                    if (val != null)
                    {
                        if (_arr == null)
                            _arr = new List<LuaValue>();

                        _arr.Add(val);
                        ExpandArray();
                    }

                    return;
                }
            }

            if (val.Value != null)
            {
                if (_map == null)
                    _map = new Dictionary<object, LuaValue>();

                _map[key.Value] = val;
            }
            else
            {
                _map.Remove(key.Value);
            }
        }

        public bool HasMetaField(string fieldName)
        {
            return MetaTable?.Get(fieldName) != null;
        }

        private void ShrinkArray()
        {
            if (_arr != null)
            {
                for (var i = Len(); i >= 0; i--)
                {
                    if (_arr[i] == null)
                        _arr.RemoveAt(i);
                }
            }
        }

        private void ExpandArray()
        {
            for (var idx = Len() + 1;; idx++)
            {
                if (_map != null && _map.ContainsKey(idx))
                {
                    var val = _map[idx];
                    _map.Remove(idx);
                    _arr.Add(val);
                }
                else
                {
                    break;
                }
            }
        }
    }
}