using System;

namespace CsLua.API
{
    /// <summary>
    /// Lua State 接口
    /// 通过下面提供的 API，Lua 可以嵌入到其他宿主语言中
    /// </summary>
    public unsafe interface ILuaState
    {
        #region state manipulation

        // --------------------------
        // ----- Lua 状态机操作 -----
        // state manipulation
        // --------------------------

        /// <summary>
        /// Sets a new panic function and returns the old one 
        /// (see https://www.lua.org/manual/5.3/manual.html#4.6)
        /// [-0, +0, –]
        /// </summary>
        LuaCSFunction AtPanic(LuaCSFunction panicF);

        /// <summary>
        /// Returns the address of the version number (a C static variable) 
        /// stored in the Lua core. When called with a valid lua_State, 
        /// returns the address of the version used to create that state. 
        /// When called with NULL, returns the address of the version running the call.
        /// [-0, +0, –]
        /// </summary>
        LuaFloat Version();

        #endregion

        #region basic stack manipulation

        // --------------------------
        // ----- 基础栈操作方法 -----
        // basic stack manipulation
        // --------------------------

        /// <summary>
        /// Converts the acceptable index idx into an equivalent 
        /// absolute index (that is, one that does not depend on the stack top).
        /// [-0, +0, –]
        /// </summary>
        int AbsIndex(int idx);

        /// <summary>
        /// Returns the index of the top element in the stack. 
        /// Because indices start at 1, this result is equal to 
        /// the number of elements in the stack; in particular, 
        /// 0 means an empty stack.
        /// [-0, +0, –]
        /// </summary>
        int GetTop();

        /// <summary>
        /// Accepts any index, or 0, and sets the stack top to this index. 
        /// If the new top is larger than the old one, then the new elements 
        /// are filled with nil. If index is 0, then all stack elements are 
        /// removed.
        /// [-?, +?, –]
        /// </summary>
        void SetTop(int idx);

        /// <summary>
        /// Pushes a copy of the element at the given index onto the stack.
        /// [-0, +1, –]
        /// </summary>
        void PushValue(int idx);

        /// <summary>
        /// Rotates the stack elements between the valid index idx and the top 
        /// of the stack. The elements are rotated n positions in the direction 
        /// of the top, for a positive n, or -n positions in the direction of 
        /// the bottom, for a negative n. The absolute value of n must not be 
        /// greater than the size of the slice being rotated. This function 
        /// cannot be called with a pseudo-index, because a pseudo-index is not
        ///  an actual stack position.
        /// [-0, +0, –]
        /// </summary>
        void Rotate(int idx, int n);

        /// <summary>
        /// Copies the element at index fromidx into the valid index toidx,
        ///  replacing the value at that position. Values at other positions 
        /// are not affected.
        /// [-0, +0, –]
        /// </summary>
        void Copy(int fromIdx, int toIdx);

        /// <summary>
        /// Ensures that the stack has space for at least n extra slots 
        /// (that is, that you can safely push up to n values into it). 
        /// It returns false if it cannot fulfill the request, either 
        /// because it would cause the stack to be larger than a fixed 
        /// maximum size (typically at least several thousand elements) 
        /// or because it cannot allocate memory for the extra space. 
        /// This function never shrinks the stack; if the stack already 
        /// has space for the extra slots, it is left unchanged.
        /// [-0, +0, –]
        /// </summary>
        bool CheckStack(int n);

        /// <summary>
        /// Exchange values between different threads of the same state.
        /// This function pops n values from the stack from, and pushes 
        /// them onto the stack to.
        /// [-?, +?, –]
        /// </summary>
        void XMove(ILuaState to, int n);

        // TODO 
        void Pop(int n);
        // TODO
        void Replace(int idx);
        // TODO
        void Insert(int idx);
        // TODO
        void Remove(int idx);

        #endregion

        #region access functions (stack -> C)

        // --------------------------
        // ----- 访问操作 -----
        // access functions (stack -> C)
        // --------------------------

        /// <summary>
        /// Returns 1 if the value at the given index is a number or a string 
        /// convertible to a number, and 0 otherwise.
        /// [-0, +0, –]
        /// </summary>
        bool IsNumber(int idx);

        /// <summary>
        /// Returns 1 if the value at the given index is a string or a number 
        /// (which is always convertible to a string), and 0 otherwise.
        /// [-0, +0, –]
        /// </summary>
        bool IsString(int idx);

        /// <summary>
        /// Returns 1 if the value at the given index is a CS function, and 0 otherwise.
        /// [-0, +0, –]
        /// </summary>
        bool IsCSFunction(int idx);

        /// <summary>
        /// Returns 1 if the value at the given index is an integer 
        /// (that is, the value is a number and is represented as an integer),
        /// and 0 otherwise.
        /// [-0, +0, –]
        /// </summary>
        bool IsInteger(int idx);

        /// <summary>
        /// Returns 1 if the value at the given index is a userdata 
        /// (either full or light), and 0 otherwise.
        /// [-0, +0, –]
        /// </summary>
        bool IsUserdata(int idx);

        /// <summary>
        /// Returns the type of the value in the given valid index, or 
        /// LUA_TNONE for a non-valid (but acceptable) index. The types 
        /// returned by lua_type are coded by the following constants defined 
        /// in lua.h: LUA_TNIL (0), LUA_TNUMBER, LUA_TBOOLEAN, LUA_TSTRING, 
        /// LUA_TTABLE, LUA_TFUNCTION, LUA_TUSERDATA, LUA_TTHREAD, and 
        /// LUA_TLIGHTUSERDATA.
        /// [-0, +0, –]
        /// </summary>
        ELuaType Type(int idx);

        /// <summary>
        /// Returns the name of the type encoded by the value tp, 
        /// which must be one the values returned by lua_type.
        /// [-0, +0, –]
        /// </summary>
        string TypeName(ELuaType tp);

        /// <summary>
        /// Converts the Lua value at the given index to the C type lua_Number 
        /// (see lua_Number). The Lua value must be a number or a string 
        /// convertible to a number 
        /// (see https://www.lua.org/manual/5.3/manual.html#3.4.3); 
        /// otherwise, lua_tonumberx returns 0.
        /// [-0, +0, –]
        /// </summary>
        LuaFloat ToNumberX(int idx, out bool isNum);

        /// <summary>
        /// Converts the Lua value at the given index to the signed integral
        ///  type lua_Integer. The Lua value must be an integer, or a number or
        ///  string convertible to an integer 
        /// (see https://www.lua.org/manual/5.3/manual.html#3.4.3); 
        /// otherwise, lua_tointegerx returns 0.
        /// [-0, +0, –]
        /// </summary>
        LuaInt ToIntegerX(int idx, out bool isInt);

        /// <summary>
        /// Converts the Lua value at the given index to a C boolean value 
        /// (0 or 1). Like all tests in Lua, lua_toboolean returns true for 
        /// any Lua value different from false and nil; otherwise it returns
        /// false. (If you want to accept only actual boolean values, 
        /// use lua_isboolean to test the value's type.)
        /// [-0, +0, –]
        /// </summary>
        bool ToBoolean(int idx);

        /// <summary>
        /// Converts the Lua value at the given index to a C string. 
        /// If len is not NULL, it sets *len with the string length. 
        /// The Lua value must be a string or a number; otherwise, 
        /// the function returns NULL. If the value is a number, 
        /// then lua_tolstring also changes the actual value in the 
        /// stack to a string. (This change confuses lua_next when 
        /// lua_tolstring is applied to keys during a table traversal.)
        /// [-0, +0, m]
        /// </summary>
        string? ToString(int idx);

        /// <summary>
        /// Returns the raw "length" of the value at the given index: 
        /// for strings, this is the string length; for tables, 
        /// this is the result of the length operator ('#') with no 
        /// metamethods; for userdata, this is the size of the 
        /// block of memory allocated for the userdata; for other values, 
        /// it is 0.
        /// [-0, +0, –]
        /// </summary>
        uint RawLen(int idx);

        /// <summary>
        /// Converts a value at the given index to a C function. 
        /// That value must be a C function; otherwise, returns NULL.
        /// [-0, +0, –]
        /// </summary>
        LuaCSFunction? ToCSFunction(int idx);

        /// <summary>
        /// If the value at the given index is a full userdata,
        /// returns its block address. If the value is a light userdata,
        /// returns its pointer. Otherwise, returns NULL
        /// [-0, +0, –]
        /// </summary>
        object? ToUserdata(int idx);

        /// <summary>
        /// Converts the value at the given index to a Lua thread 
        /// (represented as lua_State*). This value must be a thread; 
        /// otherwise, the function returns NULL.
        /// [-0, +0, –]
        /// </summary>
        ILuaState? ToThread(int idx);

        /// <summary>
        /// Converts the value at the given index to a generic 
        /// C pointer (void*). The value can be a userdata, a table, 
        /// a thread, or a function; otherwise, lua_topointer returns NULL. 
        /// Different objects will give different pointers. There is no way to 
        /// convert the pointer back to its original value.
        /// [-0, +0, –]
        /// </summary>
        ref object? ToPointer(int idx);

        #endregion

        #region Comparison and arithmetic functions

        // --------------------------
        // ----- 比较和算数操作 -----
        // Comparison and arithmetic functions
        // --------------------------

        /// <summary>
        /// Performs an arithmetic or bitwise operation over the two values 
        /// (or one, in the case of negations) at the top of the stack, 
        /// with the value at the top being the second operand, 
        /// pops these values, and pushes the result of the operation. 
        /// The function follows the semantics of the corresponding 
        /// Lua operator (that is, it may call metamethods).
        /// [-(2|1), +1, e]
        /// </summary>
        void Arith(EArithOp op);

        bool RawEqual(int idx1, int idx2);

        bool Compare(int idx1, int idx2, ECompOp op);

        #endregion

        bool IsNone(int idx);
        bool IsNil(int idx);
        bool IsNoneOrNil(int idx);
        bool IsBoolean(int idx);
        bool IsTable(int idx);
        bool IsThread(int idx);
        bool IsArray(int idx);
        // TODO
        bool IsFunction(int idx);

        LuaInt ToInteger(int idx);
        LuaFloat ToNumber(int idx);
        string? ToStringX(int idx, out bool isStr);

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