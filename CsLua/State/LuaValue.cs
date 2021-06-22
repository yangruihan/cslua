using System;
using System.Globalization;
using CsLua.API;
using CsLua.Number;

namespace CsLua.State
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    class LuaValue
    {
        public static readonly LuaValue Nil = new LuaValue(null, ELuaType.Nil);
        public static readonly LuaValue True = new LuaValue(true);
        public static readonly LuaValue False = new LuaValue(false);

        public static LuaValue Create(object value)
        {
            if (value == null)
                return Nil;
            else if (value is bool b)
                return b ? True : False;
            else
                return new LuaValue(value);
        }

        public static void SetMetaTable(LuaValue val, LuaTable mt, LuaState ls)
        {
            if (val.IsTable())
            {
                var lt = val.GetTableValue();
                lt.MetaTable = mt;
                return;
            }

            var key = $"_MT{val.Type.GetParentType()}";
            ls.Registry.Put(key, mt);
        }

        public static LuaTable GetMetaTable(LuaValue val, LuaState ls)
        {
            if (val.IsTable())
                return (val.GetTableValue()).MetaTable;

            var key = $"_MT{val.Type.GetParentType()}";
            var mt = ls.Registry.Get(key);
            return mt?.GetTableValue();
        }

        public static bool CallMetaMethod(LuaValue a, LuaValue b, string mmName, LuaState ls, out LuaValue ret)
        {
            ret = null;

            var mm = GetMetaField(a, mmName, ls);
            if (mm == null)
            {
                mm = GetMetaField(b, mmName, ls);
                if (mm == null)
                    return false;
            }

            ls.Stack.Check(4);
            ls.Stack.Push(mm);
            ls.Stack.Push(a);
            ls.Stack.Push(b);
            ls.Call(2, 1);
            ret = ls.Stack.Pop();
            return true;
        }

        public static LuaValue GetMetaField(LuaValue val, string fieldName, LuaState ls)
        {
            var mt = GetMetaTable(val, ls);
            return mt?.Get(fieldName);
        }

        private readonly object _objValue;
        private readonly LuaFloat _numValue;
        private readonly bool _boolValue;

        public ELuaType Type { get; }

        public LuaValue()
        {
            Type = ELuaType.None;
        }

        public LuaValue(LuaInt value) : this()
        {
            _numValue = BitConverter.Int64BitsToDouble(value);
            _objValue = null;
            _boolValue = false;
            Type = ELuaType.Int;
        }

        public LuaValue(LuaFloat value) : this()
        {
            _numValue = value;
            _objValue = null;
            _boolValue = false;
            Type = ELuaType.Float;
        }

        public LuaValue(bool value) : this()
        {
            _boolValue = value;
            _numValue = 0;
            _objValue = null;
            Type = ELuaType.Boolean;
        }

        private LuaValue(object value) : this()
        {
            _numValue = 0;
            _boolValue = false;
            _objValue = null;

            if (value is LuaInt l)
            {
                _numValue = BitConverter.Int64BitsToDouble(l);
                Type = ELuaType.Int;
            }
            else if (value is LuaFloat f)
            {
                _numValue = f;
                Type = ELuaType.Float;
            }
            else if (value is string)
            {
                _objValue = value;
                Type = ELuaType.String;
            }
            else if (value is LuaTable)
            {
                _objValue = value;
                Type = ELuaType.Table;
            }
            else if (value is Closure c)
            {
                Type = c.CSFunction != null ? ELuaType.CSFunction : ELuaType.Closure;
                _objValue = c;
            }
            else if (value is LuaState)
            {
                _objValue = value;
                Type = ELuaType.Thread;
            }
            else
            {
                _objValue = value;
                Type = ELuaType.LightUserData;
            }
        }

        public LuaValue(object value, ELuaType type) : this()
        {
            _objValue = value;
            _boolValue = false;
            _numValue = 0;
            Type = type;
        }

        public bool IsValid()
        {
            return Type != ELuaType.None;
        }

        public bool IsNil()
        {
            return Type == ELuaType.Nil;
        }

        public bool IsBool()
        {
            return Type == ELuaType.Boolean;
        }

        public bool IsLightUserData()
        {
            return Type == ELuaType.LightUserData;
        }

        public bool IsNumber()
        {
            return Type.IsNumber();
        }

        public bool IsInt()
        {
            return Type == ELuaType.Int;
        }

        public bool IsFloat()
        {
            return Type == ELuaType.Float;
        }

        public bool IsString()
        {
            return Type == ELuaType.String;
        }

        public bool IsTable()
        {
            return Type == ELuaType.Table;
        }

        public bool IsFunction()
        {
            return Type.IsFunction();
        }

        public bool IsClosure()
        {
            return Type == ELuaType.Closure;
        }

        public bool IsCSFunction()
        {
            return Type == ELuaType.CSFunction;
        }

        public bool IsUserData()
        {
            return Type == ELuaType.UserData;
        }

        public bool IsThread()
        {
            return Type == ELuaType.Thread;
        }

        public bool GetBoolValue()
        {
            return _boolValue;
        }

        public object GetLightUserData()
        {
            return _objValue;
        }

        public LuaInt GetIntValue()
        {
            return BitConverter.DoubleToInt64Bits(_numValue);
        }

        public LuaFloat GetFloatValue()
        {
            return _numValue;
        }

        public string GetStrValue()
        {
            return GetObjValue<string>();
        }

        public LuaTable GetTableValue()
        {
            return GetObjValue<LuaTable>();
        }

        public Closure GetClosureValue()
        {
            return GetObjValue<Closure>();
        }

        public CSFunction GetCSFunctionValue()
        {
            return GetObjValue<Closure>()?.CSFunction;
        }

        public LuaState GetThreadValue()
        {
            return GetObjValue<LuaState>();
        }

        public object GetObjValue()
        {
            return _objValue;
        }

        public object GetValue()
        {
            if (IsBool())
                return _boolValue;
            else if (IsNumber())
                return _numValue;

            return _objValue;
        }

        private T GetObjValue<T>() where T : class
        {
            return _objValue as T;
        }

        public override string ToString()
        {
            if (IsInt())
                return GetIntValue().ToString(CultureInfo.CurrentCulture);
            else if (IsFloat())
                return _numValue.ToString(CultureInfo.CurrentCulture);
            else if (IsBool())
                return _boolValue.ToString(CultureInfo.CurrentCulture);

            return _objValue == null ? "null" : _objValue.ToString();
        }

        public bool ToBoolean()
        {
            if (IsBool())
                return _boolValue;

            else if (IsNil())
                return false;

            return true;
        }

        public bool ToFloat(out LuaFloat ret)
        {
            if (IsNumber())
            {
                ret = _numValue;
                return true;
            }
            else if (IsString())
            {
                return LuaFloat.TryParse((string) _objValue, out ret);
            }

            ret = 0;
            return false;
        }

        public bool ToInteger(out LuaInt ret)
        {
            if (IsInt())
            {
                ret = BitConverter.DoubleToInt64Bits(_numValue);
                return true;
            }
            else if (IsFloat())
            {
                return LuaMath.FloatToInteger(_numValue, out ret);
            }
            else if (IsString())
            {
                return StringToInteger((string) _objValue, out ret);
            }

            ret = 0;
            return false;
        }

        private bool StringToInteger(string s, out LuaInt ret)
        {
            if (Parser.ParseInteger(s, out ret))
                return true;

            if (Parser.ParseFloat(s, out var f))
                return LuaMath.FloatToInteger(f, out ret);

            ret = 0;
            return false;
        }
    }
}