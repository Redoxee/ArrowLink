using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopAndGrabParticle : MonoBehaviour
{
    private Vector3 m_targetPosition;
    private AnimationCurve m_forceCurveFactor = null;
    private float m_attractionForce = 1f;
    [Range(0,1)]
    private float m_drag = 0;

    

}
