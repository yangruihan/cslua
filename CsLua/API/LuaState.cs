using System;

namespace CsLua.API
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    public delegate int CSFunction(ILuaState luaState);

    /// <summary>
    /// Lua State 接口
    /// 通过下面提供的 API，Lua 可以嵌入到其他宿主语言中
    /// </summary>
    public interface ILuaState
    {
        // ----- 基础栈操作方法 -----
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

        // ----- 访问操作 -----
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
        bool IsUserdata(int idx);
        bool IsArray(int idx);

        bool ToBoolean(int idx);
        Int64 ToInteger(int idx);
        bool ToIntegerX(int idx, out Int64 ret);
        double ToNumber(int idx);
        bool ToNumberX(int idx, out double ret);
        string ToString(int idx);
        bool ToStringX(int idx, out string ret);
        object ToUserdata(int idx);

        // ----- 将值推入栈中操作 -----
        void PushNil();
        void PushBoolean(bool b);
        void PushInteger(Int64 n);
        void PushNumber(double n);
        void PushString(string s);
        void PushCSFunction(CSFunction f);
        void PushGlobalTable();
        void PushCSClosure(CSFunction f, int n);
        void PushLightUserdata(object userdata);

        // ----- 算数运算操作 -----
        void Arith(EArithOp op);
        bool Compare(int idx1, int idx2, ECompOp op);

        void Len(int idx);
        void Concat(int n);

        void NewTable();
        void CreateTable(int nArr, int nRec);
        ELuaType GetTable(int idx);
        ELuaType GetField(int idx, string key);
        ELuaType GetI(int idx, LuaInt i);
        void SetTable(int idx);
        void SetField(int idx, string k);
        void SetI(int idx, LuaInt i);

        int Load(byte[] chunk, string chunkName, string mode);
        void Call(int nArgs, int nResults);

        bool IsCSFunction(int idx);
        CSFunction ToCSFunction(int idx);

        ELuaType GetGlobal(string name);
        void SetGlobal(string name);
        void Register(string name, CSFunction f);

        int LuaUpvalueIndex(int i);

        bool GetMetaTable(int idx);
        void SetMetaTable(int idx);
        uint RawLen(int idx);
        bool RawEqual(int idx1, int idx2);
        ELuaType RawGet(int idx);
        void RawSet(int idx);
        ELuaType RawGetI(int idx, LuaInt i);
        void RawSetI(int idx, LuaInt i);

        bool Next(int idx);

        int Error();
        int PCall(int nArgs, int nResults, int msgh);
    }
}