using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonetManager {
    const int c_nb_game_at_start = 15;
    const int c_nb_game_per_day = 1;

    const string c_game_key = "monet_game";

    private int m_nbGame = 0;

    public void Initialize(bool isNewDay)
    {
        if (PlayerPrefs.HasKey(c_game_key))
        {
            m_nbGame = PlayerPrefs.GetInt(c_game_key);
        }
        else
        {
            m_nbGame = c_nb_game_at_start;
        }
    }

}
