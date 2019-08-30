using System;

namespace CsLua.API
{
    public interface ILuaState
    {
        int GetTop();
        int AbsIndex(int idx);
        bool CheckStack(int n);
        void Pop(int n);
        void Copy(int fromIdx, int toIdx);
        void PushValue(int idx);
        void Replace(int idx);
        void Insert(int idx);
        void Remove(int idx);
        void Rotate(int idx, int n);
        void SetTop(int idx);

        string TypeName(ELuaType tp);
        ELuaType Type(int idx);

        bool IsNone(int idx);
        bool IsNil(int idx);
        bool IsNoneOrNil(int idx);
        bool IsBoolean(int idx);
        bool IsInteger(int idx);
        bool IsNumber(int idx);
        bool IsString(int idx);
        bool IsTable(int idx);
        bool IsThread(int idx);
        bool IsFunction(int idx);

        bool ToBoolean(int idx);
        Int64 ToInteger(int idx);
        bool ToIntegerX(int idx, out Int64 ret);
        double ToNumber(int idx);
        bool ToNumberX(int idx, out double ret);
        string ToString(int idx);
        bool ToStringX(int idx, out string ret);

        void PushNil();
        void PushBoolean(bool b);
        void PushInteger(Int64 n);
        void PushNumber(double n);
        void PushString(string s);
    }
}