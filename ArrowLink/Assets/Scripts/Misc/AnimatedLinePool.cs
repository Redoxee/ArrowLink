using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimatedLinePool : MonoBehaviour {
    
    const int c_poolSize = 50;
    [SerializeField]
    GameObject m_linePrefab = null;
    GameObject[] m_goPool = new GameObject[c_poolSize];
    RoundedLineAnimation[] m_cPool = new RoundedLineAnimation[c_poolSize];
    Dictionary<int, int> m_idToIndex = new Dictionary<int, int>(c_poolSize);
    List<int> m_availableObject = new List<int>(c_poolSize);

    private void Awake()
    {
        for (int i = 0; i < c_poolSize; ++i)
        {
            GameObject instance = Instantiate<GameObject>(m_linePrefab);
            m_goPool[i] = instance;
            var component = instance.GetComponent<RoundedLineAnimation>();
            m_cPool[i] = component;
            int id = instance.GetInstanceID();
            m_idToIndex[id] = i;
            m_availableObject.Add(id);
            instance.transform.SetParent(transform, false);
        }
    }

    public void GetInstance(out GameObject asObject, out RoundedLineAnimation asComponent)
    {
        Debug.Assert(m_availableObject.Count > 0, "Pool not big engouth !!");
        int id = m_availableObject[m_availableObject.Count - 1];
        int index = m_idToIndex[id];
        asObject = m_goPool[index];
        asComponent = m_cPool[index];
        m_availableObject.RemoveAt(m_availableObject.Count - 1);
    }

    public void FreeInstance(GameObject asObject)
    {
        int id = asObject.GetInstanceID();
        Debug.Assert(m_idToIndex.ContainsKey(id), "Unkown instance");
        m_availableObject.Add(id);
    }

    public float GetLineAnimationDuration()
    {
        return m_cPool[0].AnimationDuration;
    }
}
