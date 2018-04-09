using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class SpriteColorCycler : MonoBehaviour {

    [SerializeField]
    private Gradient m_gradient = null;

    [SerializeField]
    private float m_speed = .5f;

    private Image m_image;

    private void Awake()
    {
        m_image = GetComponent<Image>();
    }

	void Update () {
        float t = Time.time * m_speed % 1f;
        m_image.color = m_gradient.Evaluate(t);
	}

    public bool IsAnimating
    {
        set
        {
            enabled = value;
            if (!value)
            {
                m_image.color = m_gradient.Evaluate(0);
            }
        }
    }
}
