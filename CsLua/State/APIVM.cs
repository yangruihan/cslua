using System.Collections.Generic;
using CsLua.API;

namespace CsLua.State
{
    internal partial class LuaState : ILuaVM
    {
        public int PC()
        {
            return CallInfo.LuaClosure.SavedPc;
        }

        public void AddPC(int n)
        {
            CallInfo.LuaClosure.SavedPc += n;
        }

        public uint Fetch()
        {
            var i = CallInfo.LuaClosure.Closure.Proto.Code[PC()];
            AddPC(1);
            return i;
        }

        public void GetConst(int idx)
        {
            var c = _VM.GetCurrentCIClosure(this)!.Proto.Constatns[idx];
            Stack.Push(c);
        }

        public void GetRK(int rk)
        {
            if (rk > 0xff)
                GetConst(rk & 0xff);
            else
                PushValue(rk + 1);
        }

        public int RegisterCount()
        {
            return (int)_VM.GetCurrentCIClosure(this)!.Proto.MaxStackSize;
        }

        public void LoadVararg(int count)
        {
            if (count < 0)
                count = CallInfo.LuaClosure.Varargs.Length;

            CheckStack(count);
            Stack.PushN(CallInfo.LuaClosure.Varargs, count);
        }

        public void LoadProto(int idx)
        {
            var proto = CallInfo.LuaClosure.Closure.Proto.Protos[idx];
            var closure = new LuaClosure(proto);
            Stack.Push(closure);

            for (var i = 0; i < proto.Upvalues.Length; i++)
            {
                var uvInfo = proto.Upvalues[i];
                if (uvInfo.Instack == 1)
                {
                    if (CallInfo.LuaClosure.Openuvs is null)
                        CallInfo.LuaClosure.Openuvs = new Dictionary<int, Upvalue>();

                    if (CallInfo.LuaClosure.Openuvs.ContainsKey(uvInfo.Idx))
                    {
                        closure.Upvals[i] = CallInfo.LuaClosure.Openuvs[uvInfo.Idx];
                    }
                    else
                    {
                        closure.Upvals[i] = new Upvalue { Val = Stack.Slots[uvInfo.Idx] };
                        CallInfo.LuaClosure.Openuvs[uvInfo.Idx] = closure.Upvals[i];
                    }
                }
                else
                {
                    closure.Upvals[i] = CallInfo.LuaClosure.Closure.Upvals[uvInfo.Idx];
                }
            }
        }

        public void FinishCall(int first, int n)
        {
            PosCall(CallInfo, first, n);
        }

        public void CloseUpvalues(int a)
        {
            for (var i = 0; i < CallInfo.LuaClosure.Openuvs.Count; i++)
            {
                if (i >= a - 1)
                {
                    CallInfo.LuaClosure.Openuvs.Remove(i);
                }
            }
        }
    }
}