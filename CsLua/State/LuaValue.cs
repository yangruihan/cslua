using System;
using CsLua.API;
using CsLua.Common;

namespace CsLua.State
{
    class LuaValue
    {
        public static LuaValue Nil = new LuaValue(null);
        public static LuaValue True = new LuaValue(true);
        public static LuaValue False = new LuaValue(false);

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
    }
}