//#define AMG_UNLOCK_GAME

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonetManager {
    const int c_nb_game_at_start = 15;
    const int c_nb_game_per_day = 2;

    const string c_game_key = "monet_game";

    private int m_nbGame = 0;
    public int NbGame { get { return Mathf.Max(m_nbGame,0); } }


    public void Initialize(bool isNewDay)
    {
        if (PlayerPrefs.HasKey(c_game_key))
        {
            m_nbGame = PlayerPrefs.GetInt(c_game_key);
            if (isNewDay)
                m_nbGame++;
        }
        else
        {
            m_nbGame = c_nb_game_at_start;
        }
        Save();

#if AMG_UNLOCK_GAME
        m_isGameUnlocked = true;
#endif
    }

    public bool ConsumeTry()
    {
        if (m_isGameUnlocked)
            return true;

        if (m_nbGame <= 0)
            return false;
        m_nbGame--;
        Save();
        return true;
    }

    public void Save()
    {
        PlayerPrefs.SetInt(c_game_key, m_nbGame);
        PlayerPrefs.Save();
    }

    private bool m_isGameUnlocked = false;
    private bool IsPremium
    {
        get { return m_isGameUnlocked; }
    }
}
