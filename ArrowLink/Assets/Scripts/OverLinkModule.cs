using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverLinkModule {

    public int OverLinkCounter = 0;

    public int GetDotBonusForCounter(int counter)
    {
        return counter / 4 + 1;
    }

    public int GetScoreBonusForCounter(int counter)
    {
        return (counter + 1) * 15;
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
