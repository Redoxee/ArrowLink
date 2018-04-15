using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{

    public class ColorModule : MonoBehaviour
    {
        private const string c_saveKey = "selected_color_index";

        public List<ColorCollection> ColorColections;
        public ColorManager ManagerRef;

        [System.NonSerialized]
        private int m_currentSelected;
        public int CurrentSelected { get { return m_currentSelected; } }

        public void SelectColors(int index)
        {
            Debug.Assert(index < ColorColections.Count, "Invalide colection index");
            if (m_currentSelected == index)
                return;
            ManagerRef.ColorCollection = ColorColections[index];
            m_currentSelected = index;

            PlayerPrefs.SetInt(c_saveKey, index);
        }


        public void Initialize()
        {
            if (PlayerPrefs.HasKey(c_saveKey))
            {
                SelectColors(PlayerPrefs.GetInt(c_saveKey));
            }
        }
    }
}