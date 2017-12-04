using System;
using System.Collections.Generic;
using UnityEngine;

public class RoundedLineAnimation : MonoBehaviour {
    const int c_totalLinePoints = 256;
    const int c_maxIndex = c_totalLinePoints - 1;

    [Header("Positions")]
    [SerializeField]
    Transform m_startPoint = null;
    [SerializeField]
    Transform m_endPoint = null;
    [SerializeField]
    float m_depth = 0;

    [Header("Curve Shape")]
    [SerializeField]
    AnimationCurve m_decalCurve = AnimationCurve.Linear(0, 0, 1, 0);
    float m_decalFactor = 1f;
    [SerializeField]
    AnimationCurve m_forwardCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField]
    AnimationCurve m_precisionCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Animation Parameters")]
    [SerializeField]
    private float m_lineTravelTime = 1f;
    public float AnimationDuration { get { return m_lineTravelTime; } }
    [SerializeField]
    [Range(1, c_totalLinePoints * 2)]
    private int m_lineLengthInPoints = 64;
    [SerializeField]
    [Range(0, 100)]
    private int m_endDelay = 0; // Framerate dependant :s

    [Header("References")]
    [SerializeField]
    LineRenderer m_lineRenderer = null;
    [SerializeField]
    GameObject m_startSprite = null;
    [NonSerialized]
    Transform m_startSpriteTransform;
    [SerializeField]
    GameObject m_endSprite = null;
    [NonSerialized]
    Transform m_endSpriteTransform;

    [NonSerialized]
    Vector3[] m_fullLinePoints;

    private float m_frontAnimationTime = -1;
    private Vector3[] m_linePoints;
    private int m_endAnimationIndex;

    private Action m_endAction = null;

    private void Awake()
    {
        m_lineRenderer.positionCount = m_lineLengthInPoints + 1;
        m_startSpriteTransform = m_startSprite.transform;
        m_endSpriteTransform = m_endSprite.transform;
        m_lineRenderer.enabled = false;
        m_startSprite.SetActive(false);
        m_endSprite.SetActive(false);
    }

    private void Update()
    {
        UpdateLine();
    }

    public void SetUpLine(Transform start, Transform end, float decalFactor = 1f, Action endAction = null, bool imediateCompute = true)
    {
        m_startPoint = start;
        m_endPoint = end;
        m_decalFactor = decalFactor;
        if (imediateCompute)
            ComputePoints(start.position, end.position, decalFactor);
        m_endAction = endAction;
    }

    public void SetUpLine(Vector3 start, Vector3 end, float decalFactor = 1f, Action endAction = null, bool imediateCompute = true)
    {
        if (imediateCompute)
            ComputePoints(start, end, decalFactor);
        m_endAction = endAction;
    }

    public void ComputePoints()
    {
        ComputePoints(m_startPoint.position, m_endPoint.position, m_decalFactor);
    }

    public void ComputePoints(Vector3 start, Vector3 end, float decalFactor)
    {
        end.z = m_depth;
        start.z = end.z;
        Vector3 decal = end - start;
        Vector3 direction = decal.normalized;
        Vector3 normal = Quaternion.AngleAxis(90, Vector3.forward) * direction;
        float decalLength = decal.magnitude * decalFactor;

        m_fullLinePoints = new Vector3[c_totalLinePoints];
        m_fullLinePoints[0] = start;
        m_fullLinePoints[c_totalLinePoints - 1] = end;
        
        for (int i = 1; i < c_totalLinePoints - 1; ++i)
        {
            float f = i / (float)c_totalLinePoints;
            f = m_precisionCurve.Evaluate(f);
            Vector3 pos = start + m_forwardCurve.Evaluate(f) * decal ;
            pos += normal * m_decalCurve.Evaluate(f) * decalLength;

            m_fullLinePoints[i] = pos;
        }

        m_startSpriteTransform.position = start;
        m_endSpriteTransform.position = start;
    }

    public bool IsReady()
    {
        return m_fullLinePoints != null && m_fullLinePoints.Length > 0;
    }

    public void SetLineRendererFull()
    {
        if (IsReady())
        {
            m_lineRenderer.positionCount = c_totalLinePoints;
            m_lineRenderer.SetPositions(m_fullLinePoints);

            SetTipScales();

            m_startSpriteTransform.position = m_fullLinePoints[0];
            m_endSpriteTransform.position = m_fullLinePoints[m_fullLinePoints.Length - 1];
            m_lineRenderer.enabled = true;
            m_startSprite.SetActive(true);
            m_endSprite.SetActive(true);
        }
    }

    private void SetTipScales()
    {
        float width = m_lineRenderer.widthMultiplier;
        Vector3 vWidth = new Vector3(width, width, width);

        if (m_endSpriteTransform == null)
        {
            m_startSpriteTransform = m_startSprite.transform;
            m_endSpriteTransform = m_endSprite.transform;
        }
        m_endSpriteTransform.localScale = vWidth;
        m_startSpriteTransform.localScale = vWidth;

    }

    private void UpdateLine()
    {
        if (m_frontAnimationTime < 0)
        {
            enabled = false;
            return;
        }
        m_frontAnimationTime += Time.deltaTime;

        int currentIndex = Mathf.FloorToInt((m_frontAnimationTime / m_lineTravelTime) * c_maxIndex);

        int currentEndIndex = Mathf.Min(currentIndex, c_maxIndex);
        int currentStartIndex = Mathf.Clamp(currentIndex - m_lineLengthInPoints, 0, c_maxIndex);
 
        m_endSpriteTransform.position = m_fullLinePoints[currentEndIndex];
        m_startSpriteTransform.position = m_fullLinePoints[currentStartIndex];

        int currentNbPoints = currentEndIndex - currentStartIndex + 1;
        System.Array.Copy(m_fullLinePoints, currentStartIndex, m_linePoints, 0, currentNbPoints);
        m_lineRenderer.positionCount = currentNbPoints;
        m_lineRenderer.SetPositions(m_linePoints);

        if ((currentIndex - m_lineLengthInPoints) > (m_endAnimationIndex))
        {
            m_frontAnimationTime = -1;
            enabled = false;

            m_startSprite.SetActive(false);
            m_endSprite.SetActive(false);
            m_lineRenderer.enabled = false;
            if (m_endAction != null) m_endAction();
        }
    }

    public void StartAnimation()
    {
        m_frontAnimationTime = 0;
        enabled = true;
        m_linePoints = new Vector3[m_lineLengthInPoints + 1];
        m_lineRenderer.SetPositions(m_linePoints);
        m_lineRenderer.positionCount = 0;
        m_endAnimationIndex = c_maxIndex + m_endDelay;

        SetTipScales();
        m_startSprite.SetActive(true);
        m_endSprite.SetActive(true);
        m_lineRenderer.enabled = true;
    }

    public System.Collections.IEnumerator StartAnimationDelayed(float f)
    {
        yield return new WaitForSeconds(f);
        StartAnimation();
    }

    private void OnDrawGizmosSelected()
    {
        if (IsReady())
        {
            //Vector3 dc = new Vector3(.2f,  .2f, 0f);
            //Vector3 du = new Vector3(.2f, -.2f, 0f);
            for (int i = 0; i < c_totalLinePoints - 1; ++i)
            {
                Vector3 cp = m_fullLinePoints[i];
                Debug.DrawLine(cp, m_fullLinePoints[i + 1],Color.blue);
                //Debug.DrawLine(cp - dc, cp + dc, Color.white);
                //Debug.DrawLine(cp - du, cp + du, Color.white);
            }
        }
    }
}
