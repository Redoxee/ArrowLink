using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DotCollection : MonoBehaviour {

    [SerializeField]
    private Transform[] m_rows = null;
    
    private GameObject[] m_dots = null;

    private BaseUITween[] m_fadeIn;
    private BaseUITween[] m_fadeOut;
    private ParticleSystem[] m_lightParticles;
    private ParticleSystem[] m_darkParticles;

    int m_dotpPerRow;

    private int m_currentDiplay = 0;
    private int m_targetDisplay = 0;

    private float m_lightGap = .25f;
    private float m_animationTimer = 0f;


    private void Awake()
    {
        int rowCount = m_rows.Length;
        int dotPerRow = m_rows[0].childCount;
        m_dotpPerRow = dotPerRow;
        int totalDotCount = rowCount * dotPerRow;
        m_dots = new GameObject[totalDotCount];

        m_fadeIn = new BaseUITween[totalDotCount];
        m_fadeOut = new BaseUITween[totalDotCount];
        m_lightParticles = new ParticleSystem[totalDotCount];
        m_darkParticles = new ParticleSystem[totalDotCount];

        for (int r = 0; r < rowCount; ++r)
        {
            var row = m_rows[r];
            for (int d = 0; d < dotPerRow; ++d)
            {
                var obj = row.GetChild(d).gameObject;
                int dotIndex = r * dotPerRow + d;
                m_dots[dotIndex] = obj;
                var tweens = obj.GetComponents<BaseUITween>();
                m_fadeIn[dotIndex] = tweens[1];
                m_fadeOut[dotIndex] = tweens[0];

                var sprite = obj.GetComponent<Image>();
                var col = sprite.color;
                col.a = 0;
                sprite.color = col;

                m_lightParticles[dotIndex] = obj.transform.GetChild(0).GetComponent<ParticleSystem>();
                m_darkParticles[dotIndex] = obj.transform.GetChild(1).GetComponent<ParticleSystem>();
                obj.SetActive(false);
            }

        }
        enabled = false;
        
    }

    public Transform GetDot(int index)
    {
        return m_dots[index].transform;
    }

    public int MaxDots { get { return m_dots.Length; } }

    public void SetNumberOfDots(int target)
    {
        Debug.AssertFormat(target <= m_dots.Length, "invalide target , not enought dot to display {0}", target);
        m_targetDisplay = target;
        enabled = true;
        m_animationTimer = 0;
        for (int i = m_currentDiplay; i < m_targetDisplay; ++i)
        {
            m_dots[i].gameObject.SetActive(true);
        }
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
            else if(m_currentDiplay > m_targetDisplay)
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

    public void LightDot(int index, bool light = true)
    {
        var particles = light ? m_lightParticles[index] : m_darkParticles[index];
        var param  = particles.main;
        param.loop = true;
        particles.Play();
    }

    public void StopAllDots()
    {
        for (int i = 0; i < m_lightParticles.Length; ++i)
        {
            SoftStopDot(i);
        }
    }

    public void SoftStopDot(int index)
    {
        var particles = m_lightParticles[index];
        var param = particles.main;
        param.loop = false;
        param = m_darkParticles[index].main;
        param.loop = false;
    }
}
