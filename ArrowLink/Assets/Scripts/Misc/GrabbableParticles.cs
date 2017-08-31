using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableParticles : MonoBehaviour {
	[SerializeField]
	private ParticleSystem m_particles = null;
	[SerializeField]
	private GameObject m_collisionPlane = null;
	[SerializeField]
	private GameObject m_attractorRef = null;

	public float m_timeBeforeGrab = 1.5f;

	private float m_timer = 0;

	//private void Awake()
	//{
	//	enabled = false;
	//	m_collisionPlane.SetActive(false);
	//}

	private void Start()
	{
		StartEffect();
	}

	public void StartEffect()
	{
		m_particles.Play();
		m_timer = m_timeBeforeGrab;
		m_collisionPlane.SetActive(false);
		m_attractorRef.SetActive(false);
		enabled = true;
	}

	private void Update()
	{
		m_timer -= Time.deltaTime;
		if (m_timer <= 0f)
		{
			m_collisionPlane.SetActive(true);
			m_attractorRef.SetActive(true);
		}
	}
}
