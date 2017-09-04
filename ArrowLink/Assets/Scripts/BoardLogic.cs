using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ArrowLink{
	public class BoardLogic {

		public const int c_row = 4;
		public const int c_col = 4;

		LogicTile[,] m_board;

		private int m_nbTilePlaced = 0;

		public BoardLogic()
		{
			m_board = new LogicTile[c_row,c_col];
			m_nbTilePlaced = 0;
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

			m_nbTilePlaced += 1;
			return tile;
		}

		public void ComputeTileNeighbor(LogicTile tile)
		{
			for (int i = 0; i < tile.m_arrowCount; ++i)
			{
				ArrowFlag dir = tile.m_arrows[i];

				int nx = tile.X, ny = tile.Y;
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

							tile.m_listLinkedTile.Add(neighbor);
                            neighbor.m_listLinkedTile.Add(tile);
						}
					}
				}
			}
		}

		public void RemoveTile(int x, int y)
		{
			Debug.Assert(m_board[x, y] != null, "trying to remove an empty slot");
            var tile = m_board[x, y];
            foreach (var entry in tile.m_linkedTile)
            {
                var direction = entry.Key.Reverse();
                entry.Value.m_linkedTile.Remove(direction);
                entry.Value.m_listLinkedTile.Remove(tile);
            }
			m_board[x, y] = null;
			m_nbTilePlaced -= 1;
		}


		public bool IsBoardFull()
		{
			return m_nbTilePlaced >= (c_col * c_row);
		}

        public LogicTile GetTile(int x, int y)
        {
            return m_board[x, y];
        }
	}
	public class LogicTile
	{
		public int X, Y;
		public ArrowFlag m_flags;
		public ArrowFlag[] m_arrows;
		public int m_arrowCount;
		public Dictionary<ArrowFlag, LogicTile> m_linkedTile = new Dictionary<ArrowFlag, LogicTile>(8);
		public List<LogicTile> m_listLinkedTile = new List<LogicTile>(8);

		public ArrowCard m_physicCardRef = null;

		public LogicTile(ArrowFlag multiFlags, int x, int y)
		{
			m_flags = multiFlags;
			m_arrows = new ArrowFlag[8];
			multiFlags.Split(ref m_arrows, ref m_arrowCount);
			X = x; Y = y;
		}

		public void GetLinkedChain(ref HashSet<LogicTile> chain)
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
			
		}
	}
}