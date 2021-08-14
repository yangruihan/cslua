using System;

namespace CsLua.API
{
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

        bool ToNumberX(int idx, out LuaFloat ret);
        bool ToIntegerX(int idx, out LuaInt ret);
        bool ToBoolean(int idx);
        string? ToString(int idx);
        uint RawLen(int idx);
        LuaCSFunction? ToCSFunction(int idx);
        object? ToUserdata(int idx);
        ILuaState? ToThread(int idx);
        ref object? ToPointer(int idx);

        // ----- 比较和算数操作 -----
        // Comparison and arithmetic functions
        void Arith(EArithOp op);
        bool RawEqual(int idx1, int idx2);

        bool IsNone(int idx);
        bool IsNil(int idx);
        bool IsNoneOrNil(int idx);
        bool IsBoolean(int idx);
        bool IsTable(int idx);
        bool IsThread(int idx);
        bool IsArray(int idx);

        LuaInt ToInteger(int idx);
        LuaFloat ToNumber(int idx);
        bool ToStringX(int idx, out string ret);

        // ----- 将值推入栈中操作 -----
        // push functions (C -> stack)
        void PushNil();
        void PushNumber(LuaFloat n);
        void PushInteger(LuaInt n);
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

        // ----- 赋值操作 -----
        // set functions (stack -> Lua)
        void SetGlobal(string name);
        void SetTable(int idx);
        void SetField(int idx, string k);
        void SetI(int idx, LuaInt i);
        void RawSet(int idx);
        void RawSetI(int idx, LuaInt i);
        void RawSetP(int idx, object p);
        void SetMetaTable(int idx);
        void SerUserValue(int idx);

        // ----- 加载及调用操作 -----
        // 'load' and 'call' functions (load and run Lua code)
        void CallK(int nArgs, int nResults, LuaKContext ctx, LuaKFunction? k);
        void Call(int nArgs, int nResults);
        EStatus PCallK(int nArgs, int nResults, int errFuncIdx, LuaKContext ctx, LuaKFunction? k);
        EStatus PCall(int nArgs, int nResults, int errFuncIdx);

        // ----- 算数运算操作 -----
        bool Compare(int idx1, int idx2, ECompOp op);

        void Len(int idx);
        void Concat(int n);

        void NewTable();

        EStatus Load(byte[] chunk, string chunkName, string mode);

        void Register(string name, LuaCSFunction f);

        int LuaUpvalueIndex(int i);

        bool Next(int idx);

        int Error();
        void Assert(bool cond);
    }
}