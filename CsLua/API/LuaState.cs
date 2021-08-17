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

        /// <summary>
        /// Pops n elements from the stack.
        /// [-n, +0, –]
        /// </summary>
        void Pop(int n);

        /// <summary>
        /// Moves the top element into the given valid index without shifting 
        /// any element (therefore replacing the value at that given index),
        /// and then pops the top element.
        /// [-1, +0, -]
        /// </summary>
        void Replace(int idx);
        
        /// <summary>
        /// Moves the top element into the given valid index, shifting up 
        /// the elements above this index to open space. This function cannot 
        /// be called with a pseudo-index, because a pseudo-index is not 
        /// an actual stack position.
        /// [-1, +1, -]
        /// </summary>
        void Insert(int idx);
        
        /// <summary>
        /// Removes the element at the given valid index, shifting down the 
        /// elements above this index to fill the gap. This function cannot 
        /// be called with a pseudo-index, because a pseudo-index is not 
        /// an actual stack position.
        /// [-1, +0, -]
        /// </summary>
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

        /// <summary>
        /// Equivalent to lua_tonumberx with isnum equal to NULL.
        /// [-0, +0, –]
        /// </summary>
        LuaFloat ToNumber(int idx);

        /// <summary>
        /// Equivalent to lua_tointegerx with isnum equal to NULL.
        /// [-0, +0, –]
        /// </summary>
        LuaInt ToInteger(int idx);

        /// <summary>
        /// Returns 1 if the given index is not valid, and 0 otherwise.
        /// [-0, +0, -]
        /// </summary>
        bool IsNone(int idx);
        
        /// <summary>
        /// Returns 1 if the value at the given index is nil, and 0 otherwise.
        /// [-0, +0, -]
        /// </summary>
        bool IsNil(int idx);
        
        /// <summary>
        /// Returns 1 if the given index is not valid or if the value at this
        /// index is nil, and 0 otherwise.
        /// [-0, +0, –]
        /// </summary>
        bool IsNoneOrNil(int idx);
        
        /// <summary>
        /// Returns 1 if the value at the given index is a boolean, 
        /// and 0 otherwise.
        /// [-0, +0, -]
        /// </summary>
        bool IsBoolean(int idx);

        /// <summary>
        /// Returns 1 if the value at the given index is a table, 
        /// and 0 otherwise.
        /// [-0, +0, –]
        /// </summary>
        bool IsTable(int idx);

        /// <summary>
        /// Returns 1 if the value at the given index is a light userdata, 
        /// and 0 otherwise.
        /// [-0, +0, –]
        /// </summary>
        bool IsLightUserData(int idx);
        
        /// <summary>
        /// Returns 1 if the value at the given index is a thread, 
        /// and 0 otherwise.
        /// [-0, +0, –]
        /// </summary>
        bool IsThread(int idx);

        /// <summary>
        /// Returns 1 if the value at the given index is a function 
        /// (either CS or Lua), and 0 otherwise.
        /// [-0, +0, –]
        /// </summary>
        bool IsFunction(int idx);

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

        /// <summary>
        /// Returns 1 if the two values in indices index1 and index2 are 
        /// primitively equal (that is, without calling the __eq metamethod).
        /// Otherwise returns 0. Also returns 0 if any of the indices are not
        /// valid.
        /// [-0, +0, –]
        /// </summary>
        bool RawEqual(int idx1, int idx2);

        /// <summary>
        /// Compares two Lua values. Returns 1 if the value at index index1 
        /// satisfies op when compared with the value at index index2, 
        /// following the semantics of the corresponding Lua operator (that is,
        /// it may call metamethods). Otherwise returns 0. Also returns 0 if 
        /// any of the indices is not valid.
        /// [-0, +0, e]
        /// </summary>
        bool Compare(int idx1, int idx2, ECompOp op);

        #endregion

        #region push functions (C -> stack)

        // ---------------------------
        // ----- 将值推入栈中操作 -----
        // push functions (C -> stack)
        // ----------------------------

        /// <summary>
        /// Pushes a nil value onto the stack.
        /// [-0, +1, –]
        /// </summary>
        void PushNil();

        /// <summary>
        /// Pushes a float with value n onto the stack.
        /// [-0, +1, –]
        /// </summary>
        void PushNumber(LuaFloat n);

        /// <summary>
        /// Pushes an integer with value n onto the stack.
        /// [-0, +1, –]
        /// </summary>
        void PushInteger(LuaInt n);

        /// <summary>
        /// Pushes the string pointed to by s with size len onto the stack.
        /// Lua makes (or reuses) an internal copy of the given string, 
        /// so the memory at s can be freed or reused immediately after the 
        /// function returns. The string can contain any binary data, 
        /// including embedded zeros.
        /// [-0, +1, m]
        /// </summary>
        string PushString(string s);

        /// <summary>
        /// This macro is equivalent to lua_pushstring, but should be used 
        /// only when s is a literal string.
        /// [-0 +1, m]
        /// </summary>
        string PushLiteral(string s);

        /// <summary>
        /// Equivalent to lua_pushfstring, except that it receives a va_list 
        /// instead of a variable number of arguments.
        /// [-0, +1, m]
        /// </summary>
        string PushFString(string fmt, params object[] args);

        /// <summary>
        /// Pushes a new CS closure onto the stack.
        /// [-n, +1, m]
        /// </summary>
        void PushCSClosure(LuaCSFunction f, int n);

        /// <summary>
        /// Pushes a boolean value with value b onto the stack.
        /// [-0, +1, –]
        /// </summary>
        void PushBoolean(bool b);

        /// <summary>
        /// Pushes a light userdata onto the stack.
        /// [-0, +1, –]
        /// </summary>
        void PushLightUserdata(object userdata);

        /// <summary>
        /// Pushes the thread represented by L onto the stack. Returns 1 
        /// if this thread is the main thread of its state.
        /// [-0, +1, –]
        /// </summary>
        bool PushThread();

        /// <summary>
        /// Pushes a CS function onto the stack. This function receives a 
        /// pointer to a CS function and pushes onto the stack a Lua value of
        /// type function that, when called, invokes the corresponding 
        /// CS function.
        /// [-0, +1, –]
        /// </summary>
        void PushCSFunction(LuaCSFunction f);

        /// <summary>
        /// Pushes the global environment onto the stack.
        /// [-0, +1, –]
        /// </summary>
        void PushGlobalTable();

        /// <summary>
        /// Sets the CS function f as the new value of global name.
        /// [-0, +0, e]
        /// </summary>
        void Register(string name, LuaCSFunction f);

        #endregion

        #region get functions (Lua -> stack)

        // ---------------------------
        // ----- 取值操作 -----
        // get functions (Lua -> stack)
        // ---------------------------

        /// <summary>
        /// Pushes onto the stack the value of the global name. 
        /// Returns the type of that value.
        /// [-0, +1, e]
        /// </summary>
        ELuaType GetGlobal(string name);

        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the value 
        /// at the given index and k is the value at the top of the stack.
        /// [-1, +1, e]
        /// </summary>
        ELuaType GetTable(int idx);

        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the value at the 
        /// given index. As in Lua, this function may trigger a metamethod for 
        /// the "index" event 
        /// (see https://www.lua.org/manual/5.3/manual.html#2.4).
        /// [-0, +1, e]
        /// </summary>
        ELuaType GetField(int idx, string key);

        /// <summary>
        /// Pushes onto the stack the value t[i], where t is the value at the 
        /// given index. As in Lua, this function may trigger a metamethod 
        /// for the "index" event
        /// (see https://www.lua.org/manual/5.3/manual.html#2.4).
        /// [-0, +1, e]
        /// </summary>
        ELuaType GetI(int idx, LuaInt i);

        /// <summary>
        /// Similar to lua_gettable, but does a raw access
        /// (i.e., without metamethods).
        /// [-1, +1, –]
        /// </summary>
        ELuaType RawGet(int idx);

        /// <summary>
        /// Pushes onto the stack the value t[n], where t is the table at 
        /// the given index. The access is raw, that is, it does not invoke 
        /// the __index metamethod.
        /// [-0, +1, –]
        /// </summary>
        ELuaType RawGetI(int idx, LuaInt i);

        /// <summary>
        /// Pushes onto the stack the value t[k], where t is the table at 
        /// the given index and k is the pointer p represented as a 
        /// light userdata. The access is raw; that is, it does not invoke 
        /// the __index metamethod.
        /// [-0, +1, –]
        /// </summary>
        ELuaType RawGetP(int idx, object p);

        /// <summary>
        /// Creates a new empty table and pushes it onto the stack. 
        /// Parameter narr is a hint for how many elements the table will 
        /// have as a sequence; parameter nrec is a hint for how many other 
        /// elements the table will have. Lua may use these hints to preallocate
        /// memory for the new table. This preallocation is useful for 
        /// performance when you know in advance how many elements the table 
        /// will have. Otherwise you can use the function lua_newtable.
        /// [-0, +1, m]
        /// </summary>
        void CreateTable(int nArr, int nRec);

        /// <summary>
        /// This function allocates a new block of memory with the given size, 
        /// pushes onto the stack a new full userdata with the block address, 
        /// and returns this address. The host program can freely use this memory.
        /// [-0, +1, m]
        /// </summary>
        IntPtr NewUserData(int size);

        /// <summary>
        /// If the value at the given index has a metatable, the function pushes 
        /// that metatable onto the stack and returns 1. Otherwise, 
        /// the function returns 0 and pushes nothing on the stack.
        /// [-0, +(0|1), –]
        /// </summary>
        bool GetMetaTable(int idx);

        /// <summary>
        /// Pushes onto the stack the Lua value associated with the full 
        /// userdata at the given index.
        /// [-0, +1, –]
        /// </summary>
        ELuaType GetUserValue(int idx);

        /// <summary>
        /// Creates a new empty table and pushes it onto the stack. 
        /// It is equivalent to lua_createtable(L, 0, 0).
        /// [-0, +1, m]
        /// </summary>
        void NewTable();

        #endregion

        #region set functions (stack -> Lua)

        // ---------------------------
        // ----- 赋值操作 -----
        // set functions (stack -> Lua)
        // ---------------------------

        /// <summary>
        /// Pops a value from the stack and sets it as the new value of 
        /// global name.
        /// [-1, +0, e]
        /// </summary>
        void SetGlobal(string name);

        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at 
        /// the given index, v is the value at the top of the stack, 
        /// and k is the value just below the top.
        /// [-2, +0, e]
        /// </summary>
        void SetTable(int idx);

        /// <summary>
        /// Does the equivalent to t[k] = v, where t is the value at the 
        /// given index and v is the value at the top of the stack.
        /// [-1, +0, e]
        /// </summary>
        void SetField(int idx, string k);

        /// <summary>
        /// Does the equivalent to t[n] = v, where t is the value at the 
        /// given index and v is the value at the top of the stack.
        /// [-1, +0, e]
        /// </summary>
        void SetI(int idx, LuaInt i);

        /// <summary>
        /// Similar to lua_settable, but does a raw assignment 
        /// (i.e., without metamethods).
        /// [-2, +0, m]
        /// </summary>
        void RawSet(int idx);

        /// <summary>
        /// Does the equivalent of t[i] = v, where t is the table at the 
        /// given index and v is the value at the top of the stack.
        /// [-1, +0, m]
        /// </summary>
        void RawSetI(int idx, LuaInt i);

        /// <summary>
        /// Does the equivalent of t[p] = v, where t is the table at the 
        /// given index, p is encoded as a light userdata, and v is the 
        /// value at the top of the stack.
        /// [-1, +0, m]
        /// </summary>
        void RawSetP(int idx, object p);

        /// <summary>
        /// Pops a table from the stack and sets it as the new metatable for 
        /// the value at the given index.
        /// [-1, +0, –]
        /// </summary>
        bool SetMetaTable(int idx);

        /// <summary>
        /// Pops a value from the stack and sets it as the new value associated
        /// to the full userdata at the given index.
        /// [-1, +0, –]
        /// </summary>
        void SerUserValue(int idx);

        #endregion

        #region 'load' and 'call' functions (load and run Lua code)

        // ---------------------------
        // ----- 加载及调用操作 -----
        // 'load' and 'call' functions (load and run Lua code)
        // ---------------------------

        /// <summary>
        /// This function behaves exactly like lua_call, but allows the 
        /// called function to yield 
        /// (see https://www.lua.org/manual/5.3/manual.html#4.7).
        /// [-(nargs + 1), +nresults, e]
        /// </summary>
        void CallK(int nArgs, int nResults, LuaKContext ctx, LuaKFunction? k);

        /// <summary>
        /// Calls a function.
        /// [-(nargs+1), +nresults, e]
        /// </summary>
        void Call(int nArgs, int nResults);

        /// <summary>
        /// This function behaves exactly like lua_pcall, but allows the called
        /// function to yield 
        /// (see https://www.lua.org/manual/5.3/manual.html#4.7).
        /// [-(nargs + 1), +(nresults|1), –]
        /// </summary>
        EStatus PCallK(int nArgs, int nResults, int errFuncIdx, LuaKContext ctx, LuaKFunction? k);

        /// <summary>
        /// Calls a function in protected mode.
        /// [-(nargs + 1), +(nresults|1), –]
        /// </summary>
        EStatus PCall(int nArgs, int nResults, int errFuncIdx);

        /// <summary>
        /// Loads a Lua chunk without running it. If there are no errors, 
        /// lua_load pushes the compiled chunk as a Lua function on top of 
        /// the stack. Otherwise, it pushes an error message.
        /// [-0, +1, –]
        /// </summary>
        EStatus Load(byte[] chunk, string chunkName, string mode);

        #endregion

        #region coroutine functions

        // ---------------------------
        // ----- 协程相关 -----
        // coroutine functions
        // ---------------------------

        /// <summary>
        /// Yields a coroutine (thread).
        /// [-?, +?, e]
        /// </summary>
        EStatus YieldK(int nResults, LuaKContext ctx, LuaKFunction? k);

        /// <summary>
        /// tarts and resumes a coroutine in the given thread L.
        /// [-?, +?, –]
        /// </summary>
        EStatus Resume(ILuaState from, int nArgs);

        /// <summary>
        /// Returns the status of the thread L.
        /// [-0, +0, –]
        /// </summary>
        EStatus Status();

        /// <summary>
        /// Returns 1 if the given coroutine can yield, and 0 otherwise.
        /// [-0, +0, –]
        /// </summary>
        bool IsYieldable();

        /// <summary>
        /// Yields a coroutine (thread).
        /// [-?, +?, e]
        /// </summary>
        EStatus Yield(int nResults);

        #endregion

        #region garbage-collection function

        // ---------------------------
        // ----- GC相关 -----
        // garbage-collection function 
        // ---------------------------

        /// <summary>
        /// Controls the garbage collector.
        /// [+0, -0, m]
        /// </summary>
        bool GC();

        #endregion

        #region miscellaneous functions

        // ---------------------------
        // ----- 其他杂项 -----
        // miscellaneous functions
        // ---------------------------

        /// <summary>
        /// Generates a Lua error, using the value at the top of the stack as 
        /// the error object. This function does a long jump, and therefore 
        /// never returns (see luaL_error).
        /// [-1, +0, v]
        /// </summary>
        int Error();

        /// <summary>
        /// Pops a key from the stack, and pushes a key–value pair from 
        /// the table at the given index (the "next" pair after the given key).
        /// If there are no more elements in the table, then lua_next 
        /// returns 0 (and pushes nothing).
        /// [-1, +(2|0), e]
        /// </summary>
        bool Next(int idx);

        /// <summary>
        /// Concatenates the n values at the top of the stack, pops them, 
        /// and leaves the result at the top. If n is 1, the result is the 
        /// single value on the stack (that is, the function does nothing);
        /// if n is 0, the result is the empty string. Concatenation is 
        /// performed following the usual semantics of Lua 
        /// (see https://www.lua.org/manual/5.3/manual.html#3.4.6).
        /// [-n, +1, e]
        /// </summary>
        void Concat(int n);

        /// <summary>
        /// Returns the length of the value at the given index. It is equivalent
        ///  to the '#' operator in Lua 
        /// (see https://www.lua.org/manual/5.3/manual.html#3.4.7) and may 
        /// trigger a metamethod for the "length" event 
        /// (see https://www.lua.org/manual/5.3/manual.html#2.4). 
        /// The result is pushed on the stack.
        /// [-0, +1, e]
        /// </summary>
        void Len(int idx);

        /// <summary>
        /// Converts the zero-terminated string s to a number, pushes that 
        /// number into the stack, and returns the total size of the string, 
        /// that is, its length plus one. The conversion can result in an 
        /// integer or a float, according to the lexical conventions of Lua 
        /// (see https://www.lua.org/manual/5.3/manual.html#3.1). 
        /// The string may have leading and trailing spaces and a sign. 
        /// If the string is not a valid numeral, returns 0 and pushes nothing. 
        /// (Note that the result can be used as a boolean, true if the 
        /// conversion succeeds.)
        /// [-0, +1, –]
        /// </summary>
        int StringToNumber(string s);

        #endregion


        int LuaUpvalueIndex(int i);

        void Assert(bool cond);
    }
}