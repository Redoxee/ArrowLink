using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{

	[System.Flags]
	public enum ArrowFlag
	{
		NONE = 0,
		N = 1,
		NE = 2,
		E = 4,
		SE = 8,
		S = 16,
		SW = 32,
		W = 64,
		NW = 128,
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

		public static bool DoHave(this ArrowFlag a, ArrowFlag b)
		{
			return (a & b) == b;
		}
	}
}