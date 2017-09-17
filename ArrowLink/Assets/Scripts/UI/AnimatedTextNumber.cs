using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class AnimatedTextNumber : MonoBehaviour {
    private Text m_text;
    public Text Text { get { return m_text; } }

    private float m_currentNumber = 0;
    private int m_displayedNumber = 0;
    private int m_targetNumber = 0;
    public int CurrentTarget { get { return m_targetNumber; } }

    public bool IsAnimating { get { return (m_displayedNumber != m_targetNumber); } }
    [SerializeField]
    private float m_speed = 15;
    [SerializeField]
    private string m_format = null;

	void Awake () {
        m_text = GetComponent<Text>();
	}

    Action m_reachedAction = null;
    public Action ReachedAction { set { m_reachedAction = value; } }

    float m_delayTimer = 0;

    public void SetNumber(int target, float delay = 0)
    {
        m_targetNumber = target;
        if (m_displayedNumber != target)
        {
            enabled = true;
        }

        m_delayTimer = delay;
    }

    public void SetDisplay(int display)
    {
        m_targetNumber = display;
        m_currentNumber = display;
        m_displayedNumber = display;
        m_delayTimer = 0;
        enabled = false;

        if (m_format != null && m_format != "")
            m_text.text = string.Format(m_format, display);
        else
            m_text.text = display.ToString();
    }

	void Update () {
        if (m_delayTimer > 0)
        {
            m_delayTimer -= Time.deltaTime;
            return;
        }
        float delta = Time.deltaTime * m_speed * (m_currentNumber < m_targetNumber ? 1 : -1);
        if (delta < Mathf.Abs(m_currentNumber - m_targetNumber))
        {
            m_currentNumber += delta;
        }
        else
        {
            m_currentNumber = m_targetNumber;
        }

        int display = Mathf.FloorToInt(m_currentNumber);
        if (m_displayedNumber != display)
        {
            m_displayedNumber = display;

            if (m_format != null && m_format != "")
                m_text.text = string.Format(m_format, display);
            else
                m_text.text = display.ToString();
        }
        if (m_displayedNumber == m_targetNumber)
        {
            enabled = false;
            if (m_reachedAction != null)
                m_reachedAction();
        }
	}
}
