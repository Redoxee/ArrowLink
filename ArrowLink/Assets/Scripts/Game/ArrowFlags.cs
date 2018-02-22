using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{

	[System.Flags]
	public enum ArrowFlag
	{
		NONE = 0,
		N	= (1 << 0),
		NE	= (1 << 1),
		E	= (1 << 2),
		SE	= (1 << 3),
		S	= (1 << 4),
		SW	= (1 << 5),
		W	= (1 << 6),
		NW	= (1 << 7),
	}


	public static class ArrowFlagExtension
	{

		public static ArrowFlag[] AllFlags = { ArrowFlag.N, ArrowFlag.NE, ArrowFlag.E, ArrowFlag.SE, ArrowFlag.S, ArrowFlag.SW, ArrowFlag.W, ArrowFlag.NW };

		public static bool Connect(this ArrowFlag a, ArrowFlag b)
		{
			switch (a)
			{
				case ArrowFlag.N:
					return b == ArrowFlag.S;
				case ArrowFlag.NE:
					return b == ArrowFlag.SW;
				case ArrowFlag.E:
					return b == ArrowFlag.W;
				case ArrowFlag.SE:
					return b == ArrowFlag.NW;
				case ArrowFlag.S:
					return b == ArrowFlag.N;
				case ArrowFlag.SW:
					return b == ArrowFlag.NE;
				case ArrowFlag.W:
					return b == ArrowFlag.E;
				case ArrowFlag.NW:
					return b == ArrowFlag.SE;
			}
			return false;
		}

		public static ArrowFlag Reverse(this ArrowFlag a)
		{
			switch (a)
			{
				case ArrowFlag.N:
					return ArrowFlag.S;
				case ArrowFlag.NE:
					return ArrowFlag.SW;
				case ArrowFlag.E:
					return ArrowFlag.W;
				case ArrowFlag.SE:
					return ArrowFlag.NW;
				case ArrowFlag.S:
					return ArrowFlag.N;
				case ArrowFlag.SW:
					return ArrowFlag.NE;
				case ArrowFlag.W:
					return ArrowFlag.E;
				case ArrowFlag.NW:
					return ArrowFlag.SE;
			}
			return ArrowFlag.NONE;
		}

		public static void GetDecal(this ArrowFlag arrow, ref int x, ref int y)
		{
			switch (arrow)
			{
				case ArrowFlag.N:
					y--;
					break;
				case ArrowFlag.NE:
					x++; y--;
					break;
				case ArrowFlag.E:
					x++;
					break;
				case ArrowFlag.SE:
					x++; y++;
					break;
				case ArrowFlag.S:
					y++;
					break;
				case ArrowFlag.SW:
					x--; y++;
					break;
				case ArrowFlag.W:
					x--;
					break;
				case ArrowFlag.NW:
					x--; y--;
					break;
			}
		}

		public static void Split(this ArrowFlag flags, ref ArrowFlag[] array, ref int count)
		{
			count = 0;
			if ((flags & ArrowFlag.N) == ArrowFlag.N)
				array[count++] = ArrowFlag.N;
			if ((flags & ArrowFlag.NE) == ArrowFlag.NE)
				array[count++] = ArrowFlag.NE;
			if ((flags & ArrowFlag.E) == ArrowFlag.E)
				array[count++] = ArrowFlag.E;
			if ((flags & ArrowFlag.SE) == ArrowFlag.SE)
				array[count++] = ArrowFlag.SE;
			if ((flags & ArrowFlag.S) == ArrowFlag.S)
				array[count++] = ArrowFlag.S;
			if ((flags & ArrowFlag.SW) == ArrowFlag.SW)
				array[count++] = ArrowFlag.SW;
			if ((flags & ArrowFlag.W) == ArrowFlag.W)
				array[count++] = ArrowFlag.W;
			if ((flags & ArrowFlag.NW) == ArrowFlag.NW)
				array[count++] = ArrowFlag.NW;
		}

        public static int NbArrows(this ArrowFlag flags)
        {
            int res = 0;
            for(int i = 0; i < 8; ++i)
            {
                if (((int)flags & 0x1 << i) != 0)
                    ++res;
            }
            return res;
        }

		public static bool DoHave(this ArrowFlag a, ArrowFlag b)
		{
			return (a & b) == b;
		}

        public static ArrowFlag Rotate(this ArrowFlag flag)
        {
            uint b = (uint)flag;
            uint one = (b & 0x1) << 7;
            b = (b & 255) >> 1;
            return (ArrowFlag)(b | one);
        }

        public static int Differences(this ArrowFlag a, ArrowFlag b)
        {
            int result = 0;
            for (int i = 0; i < 8; ++i)
            {
                if (((int)a & (0x1 << i)) == ((int)b & (0x1 << i)))
                {
                    result++;
                }
            }
            return result;
        }
        public static int Lookalike(this ArrowFlag a, ArrowFlag b)
        {
            int result = 0;
            for (int i = 0; i < 8; ++i)
            {
                if
                    (
                        (
                            (
                                a & (ArrowFlag)(0x1 << i)
                            ) &
                            (
                                b & (ArrowFlag)(0x1 << i)
                            )
                        )
                        != ArrowFlag.NONE
                    )
                {
                    result++;
                }
            }
            return result;
        }
        public static int XorDiff(this ArrowFlag a, ArrowFlag b)
        {
            int result = 0;
            for (int i = 0; i < 8; ++i)
            {
                bool hasA = ((int)a & (0x1 << i)) > 0;
                bool hasB = ((int)b & (0x1 << i)) > 0;

                if ((hasA && !hasB) || (!hasA && hasB) )
                {
                    result++;
                }
            }
            return result;
        }


    }
}