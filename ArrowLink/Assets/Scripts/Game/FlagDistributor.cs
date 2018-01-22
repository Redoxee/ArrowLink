// #define AMG_SUPER_EASY_MODE 

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

		HashSet<ArrowFlag> m_usedFlags = new HashSet<ArrowFlag>();

		private void Awake()
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
			for (int i = 0; i < m_sortedWeight.Length; ++i)
			{
				var entry = m_sortedWeight[i];
				if (pickedValue < entry.Value)
					return entry.Key;
				pickedValue -= entry.Value;
			}
			return -1;
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
			return true;
		}

        public virtual void NotifyBonusRequested()
        {
        }

		void TestRules()
		{
			const int sample = 100000;

			int[] allSample = new int[sample];
			int[] nbCounter = new int[8];
			for (int i = 0; i < 8; ++i)
				nbCounter[i] = 0;
			float accu = 0;

			for (int i = 0; i < sample; ++i)
			{
				int pick = PickNbArrow();
				nbCounter[pick - 1] += 1;
				allSample[i] = pick;
				accu += pick;
			}

			float average = accu / (float)sample;

			Debug.LogFormat("Average = {0}", average);

			string report = string.Format("distribution : [{0},{1},{2},{3},{4},{5},{6},{7}]", nbCounter[0], nbCounter[1], nbCounter[2], nbCounter[3], nbCounter[4], nbCounter[5], nbCounter[6], nbCounter[7]);
			Debug.Log(report);
		}
	}
}