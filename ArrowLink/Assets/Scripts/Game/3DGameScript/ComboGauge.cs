using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboGauge : MonoBehaviour {

	[SerializeField]
	Transform m_fill = null;

	SpriteRenderer m_fillSprite = null;

	Vector2 m_fillBaseFillSize;
	float m_halfFillWidth;

	public void Awake()
	{
		m_fillSprite = m_fill.GetComponent<SpriteRenderer>();
		m_fillBaseFillSize = m_fillSprite.size;
		m_halfFillWidth = m_fillBaseFillSize.x / 2;
	}

	public void SetProgression(float progression)
	{
		float x = m_halfFillWidth * progression - m_halfFillWidth;
		m_fill.localPosition = new Vector3(x, 0, -1);
		m_fillSprite.size = new Vector2(progression * m_fillBaseFillSize.x, m_fillBaseFillSize.y);

	}
}
