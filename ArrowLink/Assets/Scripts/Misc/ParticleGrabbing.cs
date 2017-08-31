using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ArrowLink
{
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleGrabbing : MonoBehaviour
	{

		ParticleSystem m_particleSystem = null;

		ParticleSystem.LimitVelocityOverLifetimeModule m_velocityModule;
		ParticleSystem.ExternalForcesModule m_forceModule;

		private void Awake()
		{
			m_particleSystem = GetComponent<ParticleSystem>();
			m_velocityModule = m_particleSystem.limitVelocityOverLifetime;
			m_forceModule = m_particleSystem.externalForces;
		}

		private void Reload()
		{
			m_velocityModule.enabled = true;
			m_forceModule.enabled = false;
			//m_particleSystem.limitVelocityOverLifetime = m_velocityModule;
			
		}

		void Update()
		{

		}
	}
}