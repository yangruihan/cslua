namespace CsLua.VM
{
    static class Fpb
    {
        /// <summary>
        /// converts an integer to a "floating point byte", represented as
        /// (eeeeexxx), where the real value is (1xxx) * 2^(eeeee - 1) if
        /// eeeee != 0 and (xxx) otherwise. 
        /// </summary>
        public static int Int2fb(int x)
        {
            var e = 0;
            if (x < 8)
                return x;

            while (x >= (8 << 4))
            {
                x = (x + 0xf) >> 4;
                e += 4;
            }

            while (x >= (8 << 1))
            {
                x = (x + 1) >> 1;
                e++;
            }

            return ((e + 1) << 3) | (x - 8);
        }

        public static int Fb2Int(int x)
        {
            if (x < 8)
                return x;
            else
                return ((x & 7) + 8) << (x >> 3) - 1;
        }
    }
}