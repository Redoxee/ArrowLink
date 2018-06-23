using System;
using System.Collections.Generic;
using UnityEngine;

public class PopAndGrabParticle : MonoBehaviour
{
    [SerializeField]
    private Transform m_targetTransform = null;

    private Vector3 m_targetPosition;
    [SerializeField]
    private AnimationCurve m_forceCurveFactor = null;

    [SerializeField]
    [Range(0,1)]
    private float m_drag = 0;

    private float m_timer = 0f;
    private Vector3 m_currentVelocity = Vector3.zero;

    private Action<PopAndGrabParticle> m_endAction = null;

    private void Start()
    {
        var rnd = UnityEngine.Random.Range(0f, 2f);
        rnd -= 1f;
        rnd *= .8f;
        Shoot(3 + rnd,(PopAndGrabParticle part)=>
        {
            part.gameObject.SetActive(false);
        });
    }

    public void Shoot(float force, Action<PopAndGrabParticle> endAction = null)
    {
        m_targetPosition = m_targetTransform.position;
        float angle = UnityEngine.Random.Range(0, Mathf.PI* 2);
        m_currentVelocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * force;
        m_timer = 0f;
        enabled = true;
        m_targetPosition.z = transform.position.z;
        m_endAction = endAction;
    }

    private void Update()
    {
        
        m_timer += Time.deltaTime;
        Vector3 dir = m_targetPosition - transform.position;
        m_currentVelocity = (m_currentVelocity + dir.normalized * m_forceCurveFactor.Evaluate(m_timer)) * (1f - m_drag);
        transform.position = transform.position + m_currentVelocity * Time.deltaTime;
        if (Vector3.SqrMagnitude(transform.position - m_targetPosition) < .35f)
        {
            EndAnimation();
        }
    }

    private void EndAnimation()
    {
        enabled = false;
        if (m_endAction != null)
            m_endAction(this);
    }
    private void OnDrawGizmos()
    {
        var size = new Vector3(.5f, .5f, 0f);
        Debug.DrawLine(m_targetPosition - size, m_targetPosition + size);
        size.y *= -1;
        Debug.DrawLine(m_targetPosition - size, m_targetPosition + size);
    }
}
