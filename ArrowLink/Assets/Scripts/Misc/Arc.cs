using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Arc : MonoBehaviour {

    private LineRenderer m_lineRenderer = null;

    [SerializeField]
    private Transform m_tip = null;
    [SerializeField]
    private Transform m_top = null;

    [SerializeField]
    private float m_angle = 90f;
    [SerializeField]
    private float m_radius = 1f;

    private void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
    }

    public void BuildArc()
    {
        if (m_lineRenderer == null)
        {
            Awake();
        }
        int nbPoints = m_lineRenderer.positionCount;
        Vector3[] points = new Vector3[nbPoints];
        float ra = m_angle * Mathf.Deg2Rad;
        for (int i = 0; i < nbPoints; ++i)
        {
            float a = ra * (float)i / (nbPoints - 1) - (.5f * ra);
            points[i] = new Vector3(Mathf.Cos(a) * m_radius, Mathf.Sin(a) * m_radius, 0);
        }
        m_lineRenderer.SetPositions(points);
        m_tip.localPosition = points[0];
        m_top.localPosition = points[points.Length - 1];
        Vector3 width = new Vector3(1f, 1f, 1f) * m_lineRenderer.widthMultiplier;
        m_tip.localScale = width;
        m_top.localScale = width;
    }
}
