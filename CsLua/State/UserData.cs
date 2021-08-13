using System;
using System.Runtime.InteropServices;

namespace CsLua.State
{
    internal class UserData
    {
        public LuaTable MetaTable;
        public LuaValue User;

        public int Size { get; }
        public IntPtr Memory { get; }

        public UserData(int size)
        {
            Size = size;
            Memory = Marshal.AllocHGlobal(size);
        }

        ~UserData()
        {
            Marshal.FreeHGlobal(Memory);
        }
    }
}