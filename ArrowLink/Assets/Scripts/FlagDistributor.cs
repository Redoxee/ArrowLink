using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{

	public class FlagDistributor : MonoBehaviour
	{
		//[Serializable]
		//public struct FlagEntry
		//{
		//	public int Weight;
		//	[EnumFlagsAttribute]
		//	public ArrowFlag Flags;

		//}

		//[SerializeField]
		//List<FlagEntry> m_entryList = null;
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

			m_usedFlags.Clear();
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

		public ArrowFlag PickRandomFlags(bool checkUsedFlags = true)
		{
			ArrowFlag result = ArrowFlag.NONE;

			bool isUsed = false;

			do
			{
				int nbArrow = PickNbArrow();
				for (int i = 0; i < nbArrow; ++i)
				{
					ArrowFlag flag;
					do
					{
						flag = ArrowFlagExtension.AllFlags[Mathf.RoundToInt(UnityEngine.Random.value * 7)];
					} while (result.DoHave(flag));
					result = result | flag;
				}

				if (checkUsedFlags) isUsed = m_usedFlags.Contains(result);
			} while (isUsed);

			return result;
		}

		public bool RegisterUsedFlag(ArrowFlag flag)
		{
			if (m_usedFlags.Contains(flag))
				return false;
			m_usedFlags.Add(flag);
			return true;
		}

		public bool UnregisterUsedFlag(ArrowFlag flag)
		{
			if (!m_usedFlags.Contains(flag))
				return false;
			m_usedFlags.Remove(flag);
			return false;
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