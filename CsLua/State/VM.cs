using CsLua.API;

namespace CsLua.State
{
    internal partial class LuaState
    {
        private bool EqualObj(int indexA, int indexB, LuaState? ls)
        {
            var a = Index2Addr(indexA)!;
            var b = Index2Addr(indexB)!;
            if (a.Type != b.Type) // not the same variant?
            {
                // only numbers can be equal with different variants
                if (a.Type.GetNoVariantsType() != b.Type.GetNoVariantsType() || a.Type.GetNoVariantsType() != ELuaType.Number)
                    return false;
                else
                {
                    return (a.ToInteger(out var a1) && b.ToInteger(out var b1) && a1 == b1);
                }
            }

            LuaValue? tm = null;

            switch (a.Type)
            {
                case ELuaType.Nil:
                    return true;

                case ELuaType.Int:
                    return a.GetIntValue() == b.GetIntValue();

                case ELuaType.Float:
                    return a.GetFloatValue() == b.GetFloatValue();

                case ELuaType.Boolean:
                    return a.GetBoolValue() == b.GetBoolValue();

                case ELuaType.LightUserData:
                    return a.GetObjValue() == b.GetObjValue();

                case ELuaType.LCSFunction:
                    return a.GetLCSFunctionValue() == b.GetLCSFunctionValue();

                case ELuaType.String:
                    return a.GetStrValue() == b.GetStrValue();

                case ELuaType.UserData:
                    {
                        var u1 = a.GetUserDataValue()!;
                        var u2 = b.GetUserDataValue()!;
                        if (u1 == u2)
                            return true;

                        // will try tm
                        if (u1.MetaTable != null)
                        {
                            tm = u1.MetaTable.Get(TagMethods.Get(ETagMethods.EQ));
                        }

                        if (tm == null && u2.MetaTable != null)
                        {
                            tm = u2.MetaTable.Get(TagMethods.Get(ETagMethods.EQ));
                        }

                        break;
                    }

                case ELuaType.Table:
                    {
                        var u1 = a.GetTableValue()!;
                        var u2 = a.GetTableValue()!;
                        if (u1 == u2)
                            return true;

                        // will try tm
                        if (u1.MetaTable != null)
                        {
                            tm = u1.MetaTable.Get(TagMethods.Get(ETagMethods.EQ));
                        }

                        if (tm == null && u2.MetaTable != null)
                        {
                            tm = u2.MetaTable.Get(TagMethods.Get(ETagMethods.EQ));
                        }

                        break;
                    }

                default:
                    return a.GetObjValue() == b.GetObjValue();
            }

            // no TM?, objects are different
            if (tm == null)
                return false;

            CallTM(tm, indexA, indexB, Top, true);
            return 
        }
    }
}