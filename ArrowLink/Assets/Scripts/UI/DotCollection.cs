using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DotCollection : MonoBehaviour {
    [SerializeField]
    private GameObject[] m_dots = null;

    private BaseUITween[] m_fadeIn;
    private BaseUITween[] m_fadeOut;
    private ParticleSystem[] m_particles;


    private int m_currentDiplay = 0;
    private int m_targetDisplay = 0;

    private float m_lightGap = .25f;
    private float m_animationTimer = 0f;


    private void Awake()
    {
        var count = m_dots.Length;
        m_fadeIn = new BaseUITween[count];
        m_fadeOut = new BaseUITween[count];
        m_particles = new ParticleSystem[count];

        for (int i = 0; i < count; ++i)
        {

            var obj = m_dots[i];
            var tweens = obj.GetComponents<BaseUITween>();
            m_fadeIn[i] = tweens[1];
            m_fadeOut[i] = tweens[0];

            var sprite = obj.GetComponent<Image>();
            var col = sprite.color;
            col.a = 0;
            sprite.color = col;

            m_particles[i] = obj.transform.GetChild(0).GetComponent<ParticleSystem>();
            
        }
        enabled = false;
        
    }

    public Transform GetDot(int index)
    {
        return m_dots[index].transform;
    }

    public void SetNumberOfDots(int target)
    {
        Debug.AssertFormat(target <= m_dots.Length, "invalide target , not enought dot to display {0}", target);
        m_targetDisplay = target;
        enabled = true;
        m_animationTimer = 0;
    }

    private void Update()
    {
        m_animationTimer -= Time.deltaTime;
        if (m_animationTimer < 0)
        {
            if (m_currentDiplay < m_targetDisplay)
            {
                m_fadeIn[m_currentDiplay].StartTween();
                m_currentDiplay += 1;
            }
            else
            {
                m_fadeOut[m_currentDiplay].StartTween();
                m_currentDiplay -= 1;
            }
            m_animationTimer = m_lightGap;
        }

        if (m_currentDiplay == m_targetDisplay)
        {
            enabled = false;
            return;
        }
    }

    public void LightDot(int index)
    {
        var particles = m_particles[index];
        var param  = particles.main;
        param.loop = true;
        particles.Play();
    }

    public void StopAllDots()
    {
        for (int i = 0; i < m_particles.Length; ++i)
        {
            SoftStopDot(i);
        }
    }

    public void SoftStopDot(int index)
    {
        var particles = m_particles[index];
        var param = particles.main;
        param.loop = false;
    }
}
