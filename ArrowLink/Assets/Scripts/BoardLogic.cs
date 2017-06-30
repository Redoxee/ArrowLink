using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ArrowLink{
public class BoardLogic{

		const int c_row = 4;
		const int c_col = 4;

		List<List<LogicTile>> m_board;

		public BoardLogic()
		{
			m_board = new List<List<LogicTile>> (c_col);
			for (int i = 0; i < c_col; ++i) {
				m_board [i] = new List<LogicTile> ();
			}
		}

		class LogicTile
		{}
	}
}