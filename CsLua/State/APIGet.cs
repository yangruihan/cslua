using System;
using CsLua.API;
using CsLua.Misc;

namespace CsLua.State
{
    internal partial class LuaState : ILuaState
    {
        public void NewTable()
        {
            CreateTable(0, 0);
        }

        public void CreateTable(int nArr, int nRec)
        {
            CheckStack(1);
            var t = new LuaTable(nArr, nRec);
            Push(new LuaValue(t, ELuaType.Table));
        }

        public IntPtr NewUserData(int size)
        {
            CheckStack(1);
            var v = LuaValue.CreateUserData(size);
            Push(v);
            return v.GetUserDataValue()!.Memory;
        }

        public ELuaType GetTable(int idx)
        {
            var t = GetValueByRelIdx(idx)!;
            var k = Stack.Pop()!;
            return InnerGetTable(t, k, false).GetNoVariantsType();
        }

        public ELuaType GetField(int idx, string key)
        {
            var t = GetValueByRelIdx(idx)!;
            return InnerGetTable(t, new LuaValue(key), false)
                .GetNoVariantsType();
        }

        public ELuaType RawGet(int idx)
        {
            var t = GetValueByRelIdx(idx)!;
            Check(t.IsTable(), "table expected");
            var k = Stack.Pop()!;
            return InnerGetTable(t, k, true).GetNoVariantsType();
        }

        public ELuaType RawGetI(int idx, LuaInt i)
        {
            var t = GetValueByRelIdx(idx)!;
            Check(t.IsTable(), "table expected");
            return InnerGetTable(t, new LuaValue(i), true).GetNoVariantsType();
        }

        public ELuaType RawGetP(int idx, object p)
        {
            var t = GetValueByRelIdx(idx)!;
            Check(t.IsTable(), "table expected");
            return InnerGetTable(t, new LuaValue(p, ELuaType.LightUserData), true).GetNoVariantsType();
        }

        public ELuaType GetI(int idx, LuaInt i)
        {
            var t = GetValueByRelIdx(idx)!;
            return InnerGetTable(t, new LuaValue(i), false).GetNoVariantsType();
        }

        public ELuaType GetGlobal(string name)
        {
            var t = Registry.Get(LuaConst.LUA_RIDX_GLOBALS);
            return InnerGetTable(t, new LuaValue(name, ELuaType.String), false)
                .GetNoVariantsType();
        }

        public bool GetMetaTable(int idx)
        {
            var val = GetValueByRelIdx(idx)!;
            var mt = LuaValue.GetMetaTable(val, this);
            if (mt != null)
            {
                Push(new LuaValue(mt, ELuaType.Table));
                return true;
            }

            return false;
        }

        public ELuaType GetUserValue(int idx)
        {
            var val = GetValueByRelIdx(idx)!;
            Check(val.IsFullUserData(), "full userdata expected");
            Push(val);
            return val.Type.GetNoVariantsType();
        }

        /// <summary>
        /// 内部获取 LuaTable 值方法
        /// 可以按照 Key 类型进行优化，避免 GC
        /// </summary>
        private ELuaType InnerGetTable(LuaValue t, LuaValue k, bool raw)
        {
            if (t.IsTable())
            {
                var table = t.GetTableValue()!;
                var v = table.Get(k);

                if (raw || !v.IsNil() || !table.HasMetaField("__index"))
                {
                    Stack.Push(v);
                    return v.Type;
                }
            }

            if (!raw)
            {
                var mf = LuaValue.GetMetaField(t, "__index", this);
                if (mf != null)
                {
                    if (mf.IsTable())
                    {
                        return InnerGetTable(mf, k, false);
                    }

                    if (mf.IsFunction())
                    {
                        Stack.Push(mf);
                        Stack.Push(t);
                        Stack.Push(k);
                        Call(2, 1);
                        var v = Stack[-1];
                        Check(v != null, "__index not return valid value");
                        return v!.Type;
                    }
                }
            }

            RunError("index error!");
            return ELuaType.None;
        }
    }
}