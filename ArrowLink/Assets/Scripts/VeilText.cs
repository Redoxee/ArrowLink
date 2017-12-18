using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class VeilText : MonoBehaviour {

    [SerializeField]
    private float m_shadedAlpha = .75f;
    [SerializeField]
    private float m_nonShadedAlpha = 1f;

    private Text m_text = null;

    public float Speed = .9f;

    private float m_currentValue;
    private float m_targetValue;

    void Awake()
    {
        m_text = GetComponent<Text>();
        var color = m_text.color;
        m_targetValue = color.a;
        m_currentValue = m_targetValue;
        enabled = false;
    }

    public void SetVeilState(bool isVeilOn)
    {
        SetTarget(isVeilOn ? m_shadedAlpha : m_nonShadedAlpha);
    }

    public void SetDisplay(float value)
    {
        m_currentValue = value;
        var color = m_text.color;
        color.a = value;
        m_text.color = color;
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

        var color = m_text.color;
        color.a = m_currentValue;
        m_text.color = color;
    }
}
