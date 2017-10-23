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

        public List<List<LogicTile>> GetAllChains(ref List<LogicLinkStandalone> freeLinks)
        {
            List<List<LogicTile>> chains = new List<List<LogicTile>>();
            List<LogicTile> processed = new List<LogicTile>(c_col * c_row);
            HashSet<LogicTile> temp = new HashSet<LogicTile>();
            List<float> trash = new List<float>();

            freeLinks.Clear();

            foreach (var tile in m_allPlacedTile)
            {
                if (processed.Contains(tile))
                    continue;
                List<LogicTile> chain = new List<LogicTile>();

                tile.ComputeLinkedChain(ref temp,ref chain, ref trash, ref freeLinks);
                processed.AddRange(chain);
                chains.Add(chain);
            }

            return chains;
        }

        public void GetFreeLinkFromChain(List<LogicTile> chain,ref List<LogicLinkStandalone> linkList)
        {
            linkList.Clear();
            foreach (var tile in chain)
            {
                for (int i = 0; i < tile.m_arrowCount; ++i)
                {
                    if(!tile.m_linkedTile.ContainsKey(tile.m_arrows[i]))
                    {
                        var logic = new LogicLinkStandalone(tile,tile.m_arrows[i]);
                        linkList.Add(logic);

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

        public void ComputeLinkedChain(ref HashSet<LogicTile> chain, ref List<LogicTile> chainList, ref List<float> depthList, ref List<LogicLinkStandalone> freeLinks)
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
                    else
                    {
                        freeLinks.Add(new LogicLinkStandalone(current, currentArrow));
                    }
                }
            }
        }


        public void RemoveFlag(ArrowFlag flag)
        {
            Debug.Assert((m_flags & flag) != ArrowFlag.NONE, "Trying to remove an unexisting flag !");
            Debug.Assert(!m_linkedTile.ContainsKey(flag), "Removing an linked flag not supported !");
            m_flags = (ArrowFlag)((int)m_flags - (int)flag);
            bool isShifting = false;
            for (int i = 0; i < m_arrowCount - 1; ++i)
            {
                if (m_arrows[i] == flag)
                    isShifting = true;
                if (isShifting)
                    m_arrows[i] = m_arrows[i + 1];
            }
            m_arrowCount -= 1;
            m_physicCardRef.RemoveArrow(flag);
        }
    }

    public struct LogicLinkStandalone
    {
        public LogicTile Tile;
        public ArrowFlag Direction;

        public LogicLinkStandalone(LogicTile tile, ArrowFlag direction)
        {
            Tile = tile;
            Direction = direction;
        }

        public override int GetHashCode()
        {
            return Tile.GetHashCode() << 16 | Direction.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (typeof(LogicLinkStandalone) != obj.GetType())
                return false;
            var o = (LogicLinkStandalone)obj;
            return o.Direction == Direction && Tile == o.Tile;
        }
    }
}