using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{

	public class FlagDistributor : MonoBehaviour
	{
		[Serializable]
		public struct FlagEntry
		{
			public int Weight;
			[EnumFlagsAttribute]
			public ArrowFlag Flags;
			
		}

		[SerializeField]
		List<FlagEntry> m_entryList = null;
		
	}
}