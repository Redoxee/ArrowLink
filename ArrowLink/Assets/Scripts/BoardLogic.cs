using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ArrowLink{
	public class BoardLogic {

		public const int c_row = 4;
		public const int c_col = 4;

		LogicTile[,] m_board;
        List<LogicTile> m_allPlacedTile;
        public List<LogicTile> AllTilePlaced { get { return m_allPlacedTile; } }

		private int m_nbTilePlaced = 0;

		public BoardLogic()
		{
			m_board = new LogicTile[c_row,c_col];
            m_allPlacedTile = new List<LogicTile>(c_row * c_col);
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
            m_allPlacedTile.Add(tile);
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

        public List<List<LogicTile>> GetAllChains()
        {
            List<List<LogicTile>> chains = new List<List<LogicTile>>();
            List<LogicTile> processed = new List<LogicTile>(c_col * c_row);
            HashSet<LogicTile> temp = new HashSet<LogicTile>();
            List<float> trash = new List<float>();

            foreach (var tile in m_allPlacedTile)
            {
                if (processed.Contains(tile))
                    continue;
                List<LogicTile> chain = new List<LogicTile>();

                tile.ComputeLinkedChain(ref temp,ref chain, ref trash);
                processed.AddRange(chain);
                chains.Add(chain);
            }

            return chains;
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
            m_allPlacedTile.Remove(tile);
		}

        public bool IsBoardEmpty()
        {
            return m_nbTilePlaced == 0;
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

        //public List<LogicTile> GetLinkedChain(ref HashSet<LogicTile> chain)
        //{
        //    List<LogicTile> resultList = new List<LogicTile>();
        //    chain.Add(this);
        //    resultList.Add(this);

        //    for (int i = 0; i < m_arrowCount; ++i)
        //    {
        //        var arrow = m_arrows[i];
        //        if (!m_linkedTile.ContainsKey(arrow))
        //            continue;

        //        var neighbor = m_linkedTile[arrow];
        //        if (!chain.Contains(neighbor))
        //        {

        //            resultList.AddRange(neighbor.GetLinkedChain(ref chain));
        //        }
        //    }

        //    return resultList;
        //}

        public void ComputeLinkedChain(ref HashSet<LogicTile> chain, ref List<LogicTile> chainList, ref List<float> depthList)
        {
            float currentDepth = 0f;
            chainList.Clear();
            chainList.Add(this);
            depthList.Clear();
            depthList.Add(currentDepth);
            chain.Clear();
            chain.Add(this);

            List<LogicTile> neighbors = new List<LogicTile>();
            neighbors.Add(this);
            int nCount = 1;

            while (nCount > 0)
            {
                LogicTile current = neighbors[0];
                neighbors.RemoveAt(0);
                nCount -= 1;
                currentDepth += 1f;

                for (int i = 0; i < current.m_arrowCount; ++i)
                {
                    var currentArrow = current.m_arrows[i];
                    if (current.m_linkedTile.ContainsKey(currentArrow))
                    {
                        var neighbor = current.m_linkedTile[currentArrow];
                        if (!chain.Contains(neighbor))
                        {
                            chain.Add(neighbor);
                            neighbors.Add(neighbor);
                            nCount += 1;

                            chainList.Add(neighbor);
                            depthList.Add(currentDepth);
                        }
                    }
                }
            }
        }
    }
}