using System;
using System.Collections.Generic;
using UnityEngine;

public class OverLinkModule : MonoBehaviour{

    [NonSerialized]
    public int OverLinkCounter = 0;

    public int GetDotBonusForCounter(int counter)
    {
        return counter / 3 + 1;
    }

    public int GetScoreBonusForCounter(int counter)
    {
        return (counter ) * 35;
    }

    public int GetDotBonus()
    {
        return GetDotBonusForCounter(OverLinkCounter);
    }

    public int GetScoreBonus()
    {
        return GetScoreBonusForCounter(OverLinkCounter);
    }
}
