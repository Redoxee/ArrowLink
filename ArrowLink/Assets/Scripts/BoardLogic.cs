using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ArrowLink{
	public class BoardLogic {

		public const int c_row = 4;
		public const int c_col = 4;

		LogicTile[,] m_board;

		public BoardLogic()
		{
			m_board = new LogicTile[c_row,c_col];
		}

		public bool IsFilled(int x, int y)
		{
			return m_board[x, y] != null;
		}

		public bool IsInside(int x, int y)
		{
			return !(x < 0 || y < 0) && (x < c_col && y < c_row);
		}

		public LogicTile AddTile(int x, int y, ArrowFlag flags)
		{
			Debug.Assert(m_board[x, y] == null, "trying to assign an already filled tile");

			LogicTile tile = new LogicTile(flags, x, y);
			m_board[x, y] = tile;


			for (int i = 0; i < tile.m_arrowCount; ++i)
			{
				ArrowFlag dir = tile.m_arrows[i];

				int nx = x, ny = y;
				dir.GetDecal(ref nx, ref ny);
				if (IsInside(nx, ny))
				{
					var neighbor = m_board[nx, ny];
					if (neighbor != null)
					{
						var oposite = dir.Reverse();
						if (neighbor.m_flags.DoHave(oposite))
						{
							tile.m_linkedTile[dir] = neighbor;
							neighbor.m_linkedTile[oposite] = tile;
						}
					}

				}
			}

			return tile;
		}

		public void RemoveTile(int x, int y)
		{
			Debug.Assert(m_board[x, y] != null, "trying to remove an empty slot");
			m_board[x, y] = null;
		}

		public class LogicTile
		{
			int X, Y;
			public ArrowFlag m_flags;
			public ArrowFlag[] m_arrows;
			public int m_arrowCount;
			public Dictionary<ArrowFlag, LogicTile> m_linkedTile = new Dictionary<ArrowFlag, LogicTile>(8);

			public ArrowCard m_physicCardRef = null;

			public LogicTile(ArrowFlag multiFlags,int x, int y)
			{
				m_flags = multiFlags;
				m_arrows = new ArrowFlag[8];
				multiFlags.Split(ref m_arrows, ref m_arrowCount);
				X = x; Y = y;
			}

			public HashSet<LogicTile> GetLinkedChain(ref HashSet<LogicTile> chain)
			{
				chain.Add(this);
				for (int i = 0; i < m_arrowCount; ++i)
				{
					var arrow = m_arrows[i];
					if (!m_linkedTile.ContainsKey(arrow))
						continue;

					var neighbor = m_linkedTile[arrow];
					if (!chain.Contains(neighbor))
					{
						neighbor.GetLinkedChain(ref chain);
					}
				}

				return chain;
			}
		}
	}
}