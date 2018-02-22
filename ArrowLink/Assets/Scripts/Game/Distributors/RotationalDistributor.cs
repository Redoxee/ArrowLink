using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class RotationalDistributor : RationalDistributor
    {
        [SerializeField]
        private int m_minXORDifference = 3;

        public override ArrowFlag PickRandomFlags()
        {
            ArrowFlag result =  base.PickRandomFlags();
            
            if (result.NbArrows() <= 1)
                return result;

            int count = m_usedFlags.Count;
            for (int i = count -1; i > -1; --i)
            {
                ArrowFlag comparo = m_usedFlags[i];
                if (result.XorDiff(comparo) < m_minXORDifference)
                {
                    for (int j = 0; j < 7; ++j)
                    {
                        result = result.Rotate();
                        if (result.XorDiff(comparo) >= m_minXORDifference)
                            return result;
                    }
                }
            }
            return result;
        }
    }
}