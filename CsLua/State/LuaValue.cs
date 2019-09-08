using System;
using CsLua.API;
using CsLua.Common;
using CsLua.Number;

namespace CsLua.State
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    class LuaValue
    {
        public static readonly LuaValue Nil = new LuaValue(null);
        public static readonly LuaValue True = new LuaValue(true);
        public static readonly LuaValue False = new LuaValue(false);

        public static void SetMetaTable(LuaValue val, LuaTable mt, LuaState ls)
        {
            if (val.Value is LuaTable lt)
            {
                lt.MetaTable = mt;
                return;
            }

            var key = $"_MT{val.Type()}";
            ls.Registry.Put(key, mt);
        }

        public static LuaTable GetMetaTable(LuaValue val, LuaState ls)
        {
            if (val.Value is LuaTable lt)
                return lt.MetaTable;

            var key = $"_MT{val.Type()}";
            var mt = ls.Registry.Get(key);
            return mt?.Value as LuaTable;
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

        public object Value { get; }

        public LuaValue(object value)
        {
            Value = value;
        }

        public ELuaType Type()
        {
            if (Value is null)
                return ELuaType.Nil;

            if (Value is bool)
                return ELuaType.Boolean;

            if (Value is Int64 || Value is double)
                return ELuaType.Number;

            if (Value is string)
                return ELuaType.String;

            if (Value is LuaTable)
                return ELuaType.Table;

            if (Value is Closure)
                return ELuaType.Function;

            Debug.Panic("todo!");
            return ELuaType.None;
        }

        public bool ToBoolean()
        {
            if (Value is null)
                return false;

            if (Value is bool value)
                return value;

            return true;
        }

        public bool ToFloat(out LuaFloat ret)
        {
            if (Value is LuaInt i)
            {
                ret = i;
                return true;
            }
            else if (Value is LuaFloat f)
            {
                ret = f;
                return true;
            }
            else if (Value is string s)
            {
                return LuaFloat.TryParse(s, out ret);
            }

            ret = 0;
            return false;
        }

        public bool ToInteger(out LuaInt ret)
        {
            if (Value is long i)
            {
                ret = i;
                return true;
            }
            else if (Value is LuaFloat f)
            {
                return LuaMath.FloatToInteger(f, out ret);
            }
            else if (Value is string s)
            {
                return StringToInteger(s, out ret);
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