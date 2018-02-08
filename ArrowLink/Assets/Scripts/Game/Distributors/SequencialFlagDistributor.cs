using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class SequencialFlagDistributor : FlagDistributor
    {
        [SerializeField]
        List<int> m_flagSequence = null;
        int m_cursor = 0;
        int m_bonus = 0;
        public override ArrowFlag PickRandomFlags()
        {
            int nbArrow = m_flagSequence[m_cursor] + m_bonus;

            while (m_allFlagsBynumber[nbArrow - 1].Count == 0 && nbArrow < 8)
                nbArrow +=1;

            var pool = m_allFlagsBynumber[nbArrow - 1];
            int resultIndex = Mathf.RoundToInt(UnityEngine.Random.value * (pool.Count - 1));
            m_cursor = (m_cursor + 1) % m_flagSequence.Count;
            m_bonus = 0;
            if (nbArrow == 8)
                return ArrowFlag.N | ArrowFlag.NE | ArrowFlag.E | ArrowFlag.SE | ArrowFlag.S | ArrowFlag.SW | ArrowFlag.W | ArrowFlag.NW;
            return (ArrowFlag)pool[resultIndex];
        }

        public override void NotifyBank()
        {
            m_bonus = 3;
        }

    }
}