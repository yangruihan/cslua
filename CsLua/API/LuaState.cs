using System;

namespace CsLua.API
{
    using LuaInt = System.Int64;
    using LuaFloat = System.Double;

    /// <summary>
    /// Lua State 接口
    /// 通过下面提供的 API，Lua 可以嵌入到其他宿主语言中
    /// </summary>
    public unsafe interface ILuaState
    {
        // Lua 状态机操作
        // state manipulation
        LuaCSFunction AtPanic(LuaCSFunction panicF);
        LuaFloat Version();

        // ----- 基础栈操作方法 -----
        // basic stack manipulation
        int AbsIndex(int idx);
        int GetTop();
        void SetTop(int idx);
        void PushValue(int idx);
        void Rotate(int idx, int n);
        void Copy(int fromIdx, int toIdx);
        bool CheckStack(int n);
        void XMove(ILuaState to, int n);

        void Pop(int n);
        void Replace(int idx);
        void Insert(int idx);
        void Remove(int idx);

        // ----- 访问操作 -----
        // access functions (stack -> C)
        bool IsNumber(int idx);
        bool IsString(int idx);
        bool IsFunction(int idx);
        bool IsCSFunction(int idx);
        bool IsInteger(int idx);
        bool IsUserdata(int idx);

        ELuaType Type(int idx);
        string TypeName(ELuaType tp);

        bool ToNumberX(int idx, out double ret);
        bool ToIntegerX(int idx, out Int64 ret);
        bool ToBoolean(int idx);
        string ToString(int idx);
        uint RawLen(int idx);
        LuaCSFunction ToCSFunction(int idx);
        object ToUserdata(int idx);
        ILuaState ToThread(int idx);
        object ToPointer(int idx);

        bool IsNone(int idx);
        bool IsNil(int idx);
        bool IsNoneOrNil(int idx);
        bool IsBoolean(int idx);
        bool IsTable(int idx);
        bool IsThread(int idx);
        bool IsArray(int idx);

        Int64 ToInteger(int idx);
        double ToNumber(int idx);
        bool ToStringX(int idx, out string ret);

        // ----- 将值推入栈中操作 -----
        // push functions (C -> stack)
        void PushNil();
        void PushNumber(double n);
        void PushInteger(Int64 n);
        string PushString(string s);
        string PushFString(string fmt, params object[] args);
        void PushCSClosure(LuaCSFunction f, int n);
        void PushBoolean(bool b);
        void PushLightUserdata(object userdata);
        void PushThread();

        void PushCSFunction(LuaCSFunction f);
        void PushGlobalTable();

        // ----- 取值操作 -----
        // get functions (Lua -> stack)
        ELuaType GetGlobal(string name);
        ELuaType GetTable(int idx);
        ELuaType GetField(int idx, string key);
        ELuaType GetI(int idx, LuaInt i);
        ELuaType RawGet(int idx);
        ELuaType RawGetI(int idx, LuaInt i);
        ELuaType RawGetP(int idx, object p);
        void CreateTable(int nArr, int nRec);
        IntPtr NewUserData(int size);
        bool GetMetaTable(int idx);
        ELuaType GetUserValue(int idx);

        // ----- 算数运算操作 -----
        void Arith(EArithOp op);
        bool Compare(int idx1, int idx2, ECompOp op);

        void Len(int idx);
        void Concat(int n);

        void NewTable();
        void SetTable(int idx);
        void SetField(int idx, string k);
        void SetI(int idx, LuaInt i);

        EErrorCode Load(byte[] chunk, string chunkName, string mode);
        void Call(int nArgs, int nResults);

        void SetGlobal(string name);
        void Register(string name, LuaCSFunction f);

        int LuaUpvalueIndex(int i);

        void SetMetaTable(int idx);
        bool RawEqual(int idx1, int idx2);
        void RawSet(int idx);
        void RawSetI(int idx, LuaInt i);

        bool Next(int idx);

        int Error();
        void Assert(bool cond);
        EErrorCode PCall(int nArgs, int nResults, int msgh);
    }
}