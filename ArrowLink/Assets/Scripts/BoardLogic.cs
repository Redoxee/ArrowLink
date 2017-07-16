using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ArrowLink{
	public class BoardLogic {

		const int c_row = 4;
		const int c_col = 4;

		LogicTile[,] m_board;

		ArrowFlag[] m_arrowArray = new ArrowFlag[8];
		int m_arrowCount;

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

		public void AddTile(int x, int y, ArrowFlag flags)
		{
			Debug.Assert(m_board[x, y] == null, "trying to assign an already filled tile");
			LogicTile tile = new LogicTile(flags);
			m_board[x, y] = tile;

			var allDirection = ArrowFlagExtension.AllFlags;
			foreach (var dir in allDirection)
			{
				if (flags.DoHave(dir))
				{
					int nx = x, ny = y;
					flags.GetDecal(ref nx, ref ny);
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
			}
		}

		public void RemoveTile(int x, int y)
		{
			Debug.Assert(m_board[x, y] != null, "trying to remove an empty slot");
			m_board[x, y] = null;
		}

		//List<LogicTile> ResolvFromTile(int x, int y)
		//{
		//	Debug.Assert(m_board[x][y] != null, "trying to resolve an empty slot");
		//	Stack<LogicTile> tileToExplore = new Stack<LogicTile>(); 
		//	List<LogicTile> result = new List<LogicTile> ();

		//	tileToExplore.Push(m_board[x][y]);
		//	while (tileToExplore.Count > 0)
		//	{
		//		LogicTile ct = tileToExplore.Pop();
		//		result.Add(ct);
		//		var arrows = ct.Arrows;
		//		for (int i = 0; i < ct.ArrowCount; ++i)
		//		{
		//			int dx = 0, dy = 0;
		//			arrows[i].GetDecal(ref dx, ref dy);
		//			int nx = x + dx; int ny = y + dy;
		//			if (!(nx < 0 || ny < 0 || nx >= c_col || ny >= c_row))
		//			{
		//				if (IsFilled(nx, ny))
		//				{
		//					var nt = m_board[nx][ny];
		//					if (!(tileToExplore.Contains(nt) || result.Contains(nt)))
		//					{

		//					}
		//				}
		//			}
		//		}
		//	}

		//	return result;
		//}

		class LogicTile
		{
			public ArrowFlag m_flags;
			public ArrowFlag[] m_arrows;
			public int m_arrowCount;
			public Dictionary<ArrowFlag, LogicTile> m_linkedTile = new Dictionary<ArrowFlag, LogicTile>(8);


			public LogicTile(ArrowFlag multiFlags)
			{
				m_flags = multiFlags;
				m_arrows = new ArrowFlag[8];
				multiFlags.Split(ref m_arrows, ref m_arrowCount);
			}

		}
	}
}