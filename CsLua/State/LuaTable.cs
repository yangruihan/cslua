using System;
using System.Collections.Generic;
using CsLua.API;
using CsLua.Common;
using CsLua.Number;

namespace CsLua.State
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    class LuaTable
    {
        private static object KeysHead = new Byte();

        public LuaTable MetaTable;

        private List<LuaValue> _arr;
        private Dictionary<object, LuaValue> _map;
        private Dictionary<object, LuaValue> _keys;
        private object _lastKey;
        private bool _changed;

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
            if (key.IsInt())
                idx = key.GetIntValue();
            else if (key.IsFloat())
                if (LuaMath.FloatToInteger(key.GetFloatValue(), out var fi))
                    idx = fi;

            if (idx >= 1 && idx <= Len())
                return _arr[(int) (idx - 1)];

            return _map != null && _map.TryGetValue(key.GetValue(), out var ret) ? ret : LuaValue.Nil;
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
            if (key is null || key.IsNil())
                Debug.Panic("table index is nil!");

            if (key.IsFloat() && LuaFloat.IsNaN(key.GetFloatValue()))
                Debug.Panic("table index is Nan!");

            _changed = true;

            LuaInt idx = 0;
            if (key.IsInt())
                idx = key.GetIntValue();
            else if (key.IsFloat())
                if (LuaMath.FloatToInteger(key.GetFloatValue(), out var fi))
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
                    if (_map != null && _map.ContainsKey(key.GetIntValue()))
                        _map.Remove(key.GetIntValue());

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

            if (!val.IsNil())
            {
                if (_map == null)
                    _map = new Dictionary<object, LuaValue>();

                _map[key.GetValue()] = val;
            }
            else
            {
                _map.Remove(key.GetValue());
            }
        }

        public bool HasMetaField(string fieldName)
        {
            return MetaTable?.Get(fieldName) != null;
        }

        public LuaValue NextKey(LuaValue key)
        {
            if (_keys == null || key.IsNil() && _changed)
            {
                InitKeys();
                _changed = false;
            }

            var nextKey = LuaValue.Nil;
            if (key.IsNil())
                nextKey = _keys[KeysHead];
            else if (_keys.ContainsKey(key.GetValue()))
                nextKey = _keys[key.GetValue()];

            if (nextKey.IsNil() && key.GetValue() != null && key.GetValue() != _lastKey)
            {
                Debug.Panic("invalid key to 'next'");
            }

            return nextKey;
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

        private void InitKeys()
        {
            _keys = new Dictionary<object, LuaValue>();
            object key = KeysHead;

            if (_arr != null)
            {
                for (var i = 0; i < _arr.Count; i++)
                {
                    var value = _arr[i];
                    if (value.GetValue() != null)
                    {
                        var newValue = new LuaValue((LuaInt) (i + 1));
                        ;
                        _keys.Add(key, newValue);
                        key = newValue.GetValue();
                    }
                }
            }

            if (_map != null)
            {
                foreach (var kv in _map)
                {
                    if (kv.Value != null)
                    {
                        var value = new LuaValue(kv.Key);
                        _keys.Add(key, value);
                        key = value.GetValue();
                    }
                }
            }

            _lastKey = key;
        }
    }
}