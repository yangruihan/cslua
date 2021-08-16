using System;
using System.Globalization;
using CsLua.API;
using CsLua.Number;
using System.Runtime.CompilerServices;

namespace CsLua.State
{
    internal class LuaValue
    {
        public static readonly LuaValue Nil = new LuaValue(0, ELuaType.Nil);
        public static readonly LuaValue True = new LuaValue(true);
        public static readonly LuaValue False = new LuaValue(false);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaValue Create()
        {
            return Nil;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaValue Create(bool flag)
        {
            return flag ? True : False;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaValue Create(LuaInt i)
        {
            return new LuaValue(i);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaValue Create(LuaFloat f)
        {
            return new LuaValue(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaValue Create(string s)
        {
            return new LuaValue(s);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaValue Create(object value)
        {
            return new LuaValue(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LuaValue CreateUserData(int size)
        {
            var userData = new UserData(size);
            return new LuaValue(userData, ELuaType.UserData);
        }

        public static void SetMetaTable(LuaValue val, LuaTable? mt, LuaState ls)
        {
            if (val.IsTable())
            {
                val.GetTableValue()!.MetaTable = mt;
                return;
            }

            if (val.IsFullUserData())
            {
                val.GetUserDataValue()!.MetaTable = mt;
                return;
            }

            ls.GlobalState.Mt[(int)val.Type.GetNoVariantsType()] = mt;
        }

        public static LuaTable? GetMetaTable(LuaValue val, LuaState ls)
        {
            if (val.IsTable())
                return val.GetTableValue()!.MetaTable;

            if (val.IsFullUserData())
                return val.GetUserDataValue()!.MetaTable;

            var mt = ls.GlobalState.Mt[(int)val.Type.GetNoVariantsType()];
            return mt;
        }

        public static bool CallMetaMethod(LuaValue a, LuaValue b, string mmName,
            LuaState ls, out LuaValue? ret)
        {
            ret = null;

            var mm = GetMetaField(a, mmName, ls);
            if (mm == null)
            {
                mm = GetMetaField(b, mmName, ls);
                if (mm == null)
                    return false;
            }

            ls.CheckStack(4);
            ls.Stack.Push(mm);
            ls.Stack.Push(a);
            ls.Stack.Push(b);
            ls.Call(2, 1);
            ret = ls.Stack.Pop();
            return true;
        }

        public static LuaValue? GetMetaField(LuaValue val, string fieldName,
            LuaState ls)
        {
            var mt = GetMetaTable(val, ls);
            return mt?.Get(fieldName);
        }

        private object? _objValue;
        private LuaFloat _numValue;
        private bool _boolValue;

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

        public LuaValue(string s) : this()
        {
            _objValue = s;
            Type = ELuaType.String;
        }

        public LuaValue(object value) : this()
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
            else if (value is LuaClosure c)
            {
                Type = ELuaType.LuaClosure;
                _objValue = c;
            }
            else if (value is CSClosure)
            {
                Type = ELuaType.CSClosure;
                _objValue = value;
            }
            else if (value is LuaCSFunction)
            {
                Type = ELuaType.LCSFunction;
                _objValue = value;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid()
        {
            return Type != ELuaType.None;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNil()
        {
            return Type == ELuaType.Nil;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsBool()
        {
            return Type == ELuaType.Boolean;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLightUserData()
        {
            return Type == ELuaType.LightUserData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNumber()
        {
            return Type.IsNumber();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInt()
        {
            return Type == ELuaType.Int;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFloat()
        {
            return Type == ELuaType.Float;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsString()
        {
            return Type == ELuaType.String;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsTable()
        {
            return Type == ELuaType.Table;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFunction()
        {
            return Type.IsFunction();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLuaClosure()
        {
            return Type == ELuaType.LuaClosure;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsLCSFunction()
        {
            return Type == ELuaType.LCSFunction;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCSClosure()
        {
            return Type == ELuaType.CSClosure;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsFullUserData()
        {
            return Type == ELuaType.UserData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsThread()
        {
            return Type == ELuaType.Thread;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CanConvertToStr()
        {
            return IsNumber();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetBoolValue()
        {
            return _boolValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaInt GetIntValue()
        {
            return BitConverter.DoubleToInt64Bits(_numValue);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaFloat GetFloatValue()
        {
            return _numValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? GetLightUserData()
        {
            return _objValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string? GetStrValue()
        {
            return GetObjValue<string>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaTable? GetTableValue()
        {
            return GetObjValue<LuaTable>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaClosure? GetLuaClosureValue()
        {
            return GetObjValue<LuaClosure>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaCSFunction? GetLCSFunctionValue()
        {
            return GetObjValue<LuaCSFunction>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CSClosure? GetCSClosure()
        {
            return GetObjValue<CSClosure>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaCSFunction? GetCSClosureFunctionValue()
        {
            return GetObjValue<CSClosure>()?.LuaCsFunction;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UserData? GetUserDataValue()
        {
            return GetObjValue<UserData>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LuaState? GetThreadValue()
        {
            return GetObjValue<LuaState>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object? GetObjValue()
        {
            return _objValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref bool GetBoolRefValue()
        {
            return ref _boolValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref LuaFloat GetNumRefValue()
        {
            return ref _numValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref object? GetObjRefValue()
        {
            return ref _objValue;
        }

        public object? GetValue()
        {
            if (IsBool())
                return _boolValue;
            else if (IsNumber())
                return _numValue;

            return _objValue;
        }

        private T? GetObjValue<T>() where T : class
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

            return _objValue == null ? "null" : _objValue!.ToString()!;
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
                return LuaFloat.TryParse((string)_objValue!, out ret);
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
                return StringToInteger((string)_objValue!, out ret);
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