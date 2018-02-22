using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class RationalDistributor : FlagDistributor
    {
        [Serializable]
        public struct DifficultyEntry
        {
            public int HitPoint;
            public int MaxInterval;

            public class DifficultyComparer : Comparer<DifficultyEntry>
            {
                public override int Compare(DifficultyEntry x, DifficultyEntry y)
                {
                    if (x.HitPoint == y.HitPoint)
                        return 0;
                    return x.HitPoint > y.HitPoint ? 1 : -1;
                }
            }
        }
        
        [SerializeField]
        private List<DifficultyEntry> m_difficulties = null;

        private int[] m_precedence = new int[8];

        private int m_currentDifficultyLevel = 0;

        [SerializeField]
        private DifficultyEntry m_currentDifficulty;

        protected override void Awake()
        {
            base.Awake();
            for (int i = 0; i < m_precedence.Length; ++i)
            {
                m_precedence[0] = 0;
            }

            m_difficulties.Sort(new DifficultyEntry.DifficultyComparer());
            m_currentDifficulty = GetDifficultyEntry(m_currentDifficultyLevel);
        }

        public override ArrowFlag PickRandomFlags()
        {
            ArrowFlag flags = base.PickRandomFlags();
            
            CheckArrows(flags);
            for (int i = 0; i < 8; ++i)
            {
                if (m_precedence[i] >= m_currentDifficulty.MaxInterval)
                {
                    flags = flags | (ArrowFlag)(0x1 << i);
                    m_precedence[i] = 0;
                }
            }

            return flags;
        }


        private void CheckArrows(ArrowFlag flags)
        {
            int casted = (int)flags;
            for (int i = 0; i < 8; ++i)
            {
                int current = 0x1 << i;
                if ((casted | current) > 0)
                {
                    m_precedence[i] = 0;
                }
                else
                {
                    m_precedence[i]++;
                }
            }
        }

        public override void NotifyBank()
        {
            base.NotifyBank();
            m_currentDifficultyLevel += 1;
            m_currentDifficulty = GetDifficultyEntry(m_currentDifficultyLevel);
        }

        private DifficultyEntry GetDifficultyEntry(int level)
        {
            int count = m_difficulties.Count;
            DifficultyEntry result = m_difficulties[0];
            for (int i = 1; i < count; ++i)
            {
                if (m_difficulties[i].HitPoint > level)
                    break;

                result = m_difficulties[i];
            }
            return result;
        }

    }
}
