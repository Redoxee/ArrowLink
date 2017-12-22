using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class VeilSprite : MonoBehaviour {
    [SerializeField]
    private float m_shadedAlpha = .75f;

    private Image m_image = null;
    private Color m_baseColor;

    public float Speed = .9f;

    private float m_currentValue;
    private float m_targetValue;

    void Awake()
    {
        m_image = GetComponent<Image>();
        m_baseColor = m_image.color;
        m_targetValue = m_baseColor.a;
        m_currentValue = m_targetValue;
        enabled = false;
    }

    public void SetVeilState(bool isVeilOn)
    {
        SetTarget(isVeilOn ? m_shadedAlpha: 0f);
    }

    public void SetDisplay(float value)
    {
        m_currentValue = value;
        m_baseColor.a = value;
        m_image.color = m_baseColor;
        enabled = true;
    }

    public void SetTarget(float target)
    {
        m_targetValue = target;
        enabled = true;
    }


    void Update()
    {
        float direction = m_currentValue > m_targetValue ? -1 : 1;
        float nextValue = m_currentValue + direction * Speed * Time.deltaTime;
        if ((direction > 0 && nextValue >= m_targetValue) ||
           (direction < 0 && nextValue <= m_targetValue))
        {
            m_currentValue = m_targetValue;
            enabled = false;
        }
        else
        {
            m_currentValue = nextValue;
        }

        m_baseColor.a = m_currentValue;
        m_image.color = m_baseColor;
    }
}
