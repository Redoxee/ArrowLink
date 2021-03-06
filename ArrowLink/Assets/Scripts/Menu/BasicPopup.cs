﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BasicPopup : MonoBehaviour {

    [SerializeField]
    CanvasGroup m_canvasGroup = null;

    public void Show()
    {
        m_canvasGroup.alpha = 1f;
        m_canvasGroup.interactable = true;
        m_canvasGroup.blocksRaycasts  = true;
    }


    public void Hide()
    {
        m_canvasGroup.alpha = 0f;
        m_canvasGroup.interactable = false;
        m_canvasGroup.blocksRaycasts = false;
    }
}
