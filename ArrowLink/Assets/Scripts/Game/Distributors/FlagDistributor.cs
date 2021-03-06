﻿//#define AMG_SUPER_EASY_MODE 
// #define AMG_PREDETERMINED_TILES


using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{

	public class FlagDistributor : MonoBehaviour
	{
		[SerializeField]
		int[] m_arrowNumberDistribution = new int[8];

		KeyValuePair<int, int>[] m_sortedWeight = new KeyValuePair<int, int>[8];
		int m_totalWeight;

		protected List<ArrowFlag> m_usedFlags = new List<ArrowFlag>();

        [SerializeField]
        private int m_maxOneArrow = 3;

        protected virtual void Awake()
		{
			Initialize();
			//TestRules();
		}

		private void Initialize()
		{
			m_totalWeight = 0;
			for (int i = 0; i < m_arrowNumberDistribution.Length; ++i)
			{
				m_sortedWeight[i] = new KeyValuePair<int, int>(i + 1, m_arrowNumberDistribution[i]);
				m_totalWeight += m_sortedWeight[i].Value;
			}

			Array.Sort(m_sortedWeight,
				(KeyValuePair<int, int> a, KeyValuePair<int, int> b) => { return a.Value.CompareTo(b.Value) * -1; });

			ComputeFallTiles();

			m_usedFlags.Clear();
		}


		protected List<int>[] m_allFlagsBynumber = new List<int>[8];
		protected HashSet<int> m_allFlags = new HashSet<int>();

		private void ComputeFallTiles()
		{
			m_allFlags.Clear();
			for (int i = 0; i < 8; ++i)
			{
				m_allFlagsBynumber[i] = new List<int>();
			}
			int totalNbTiles = (1 << 8) - 1;
			for (int i = 0; i < totalNbTiles; ++i)
			{

				int current = i + 1;
				m_allFlags.Add(current);
				int nbFlags = CountFlag(current);
				m_allFlagsBynumber[nbFlags - 1].Add(current);
			}
		}

		private int CountFlag(int bitField)
		{
			int nb = 0;

			for (int i = 0; i < 8; ++i)
			{
				if ((bitField & 1 << i) != 0)
					nb += 1;
			}

			return nb;
		}

		private int PickNbArrow()
		{
			int pickedValue = Mathf.RoundToInt(UnityEngine.Random.value * (m_totalWeight - 1));
            int result = -1;
			for (int i = 0; i < m_sortedWeight.Length; ++i)
			{
				var entry = m_sortedWeight[i];
                if (pickedValue < entry.Value)
                {
                    result = entry.Key;
                    break;
                }
				pickedValue -= entry.Value;
			}

            if (result < 2 && (m_allFlagsBynumber[0].Count <= (8 - m_maxOneArrow)))
            {
                return 2;
            }

            return result;
		}

		public virtual ArrowFlag PickRandomFlags()
		{
			if (m_allFlags.Count == 0)
				throw new Exception("No more flag to dispense !");

			int nbArrow = PickNbArrow();
			while(m_allFlagsBynumber[nbArrow - 1].Count == 0)
				nbArrow = PickNbArrow();
			var pool = m_allFlagsBynumber[nbArrow - 1];
			int resultIndex = Mathf.RoundToInt(UnityEngine.Random.value * (pool.Count - 1));

#if AMG_SUPER_EASY_MODE
            return ArrowFlag.N | ArrowFlag.S | ArrowFlag.E | ArrowFlag.W;
#elif AMG_PREDETERMINED_TILES
            return m_flagSequence[m_currentFlagIndex++ % m_flagSequence.Length];
#else
            return (ArrowFlag)pool[resultIndex];
#endif

        }

		public bool RegisterUsedFlag(ArrowFlag flag)
		{
			int iFlag = (int)flag;
			if (!m_allFlags.Contains(iFlag))
				return false;
			m_allFlags.Remove(iFlag);
			int nbFlags = CountFlag(iFlag);
			m_allFlagsBynumber[nbFlags - 1].Remove(iFlag);

            m_usedFlags.Add(flag);
			return true;
		}

		public bool UnregisterUsedFlag(ArrowFlag flag)
		{
			int iFlag = (int)flag;
			if (m_allFlags.Contains(iFlag))
				return false;
			int nbArrow = CountFlag(iFlag);
            if (nbArrow == 0)
                return false;
			m_allFlags.Add(iFlag);
			m_allFlagsBynumber[nbArrow - 1].Add(iFlag);

            m_usedFlags.Remove(flag);
			return true;
		}

        public virtual void NotifyBank()
        {
        }
        
        #region Predertermined sequence
#if AMG_PREDETERMINED_TILES
        private int m_currentFlagIndex = 0;
        private ArrowFlag[] m_flagSequence =
            {

                ArrowFlag.W|ArrowFlag.SW|ArrowFlag.NE|ArrowFlag.E,
                ArrowFlag.N|ArrowFlag.E|ArrowFlag.S|ArrowFlag.W,
                ArrowFlag.N|ArrowFlag.E|ArrowFlag.S|ArrowFlag.W,
                ArrowFlag.N|ArrowFlag.E|ArrowFlag.S|ArrowFlag.W,
                ArrowFlag.N|ArrowFlag.E|ArrowFlag.S|ArrowFlag.W,
                ArrowFlag.N|ArrowFlag.E|ArrowFlag.S|ArrowFlag.W,
                ArrowFlag.N|ArrowFlag.E|ArrowFlag.S|ArrowFlag.W,
                ArrowFlag.N|ArrowFlag.E|ArrowFlag.S|ArrowFlag.W,
                ArrowFlag.N|ArrowFlag.E|ArrowFlag.S|ArrowFlag.W,
                ArrowFlag.N|ArrowFlag.E|ArrowFlag.S|ArrowFlag.W,
                ArrowFlag.N|ArrowFlag.E|ArrowFlag.S|ArrowFlag.W,
                ArrowFlag.N|ArrowFlag.E|ArrowFlag.S|ArrowFlag.W,
                ArrowFlag.N|ArrowFlag.E|ArrowFlag.S|ArrowFlag.W,
                ArrowFlag.N,
                ArrowFlag.S|ArrowFlag.SE|ArrowFlag.E,
                ArrowFlag.NW|ArrowFlag.E,
                ArrowFlag.W|ArrowFlag.NW|ArrowFlag.NE|ArrowFlag.E,
                ArrowFlag.SE|ArrowFlag.NE,
                ArrowFlag.W|ArrowFlag.E,
                ArrowFlag.W,
                ArrowFlag.SW|ArrowFlag.E,
                ArrowFlag.W|ArrowFlag.NW|ArrowFlag.NE|ArrowFlag.SE,
                ArrowFlag.W|ArrowFlag.SW,
                ArrowFlag.NW|ArrowFlag.SE,
                ArrowFlag.SW|ArrowFlag.E,
                ArrowFlag.NW|ArrowFlag.N,
                ArrowFlag.N|ArrowFlag.E|ArrowFlag.S|ArrowFlag.W,

                ArrowFlag.W|ArrowFlag.NW|ArrowFlag.NE|ArrowFlag.E|ArrowFlag.S
            };
#endif
        #endregion
    }
}