using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    [RequireComponent(typeof(ParticleSystem))]
    public class LinkParticle : MonoBehaviour
    {
        ParticleSystem particle;
        private void Start()
        {
            particle = transform.GetComponent<ParticleSystem>();
            var mainParameters = particle.main;
            mainParameters.startRotationZ = Mathf.Deg2Rad * 45;
            mainParameters.startSizeY = 2;
            var colors = particle.colorOverLifetime;
        }
    }
}