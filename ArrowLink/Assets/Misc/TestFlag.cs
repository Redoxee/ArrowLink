using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ArrowLink;

public class TestFlag : FlagDistributor {

    [SerializeField]
    private GameObject m_tilePrefab = null;
    [SerializeField]
    private float m_tileSpacing = 2f;
    [SerializeField]
    private int m_nbCards = 60;
    [SerializeField]
    FlagDistributor m_refDistributor = null;

    public static TestFlag Instance = null;

    private Vector3 m_basePosition;
    private Vector3 m_currentPosition;

    void Start()
    {
        Instance = this;

        m_basePosition = new Vector3(
            0,
            0,
            0);
        m_currentPosition = m_basePosition;

        //CreateCards();
    }
    private void Update()
    {
        if (m_nbCards <= 0)
            return;
        CreateCard();
        --m_nbCards;
    }

    private void CreateCard()
    {
        var tile = Instantiate(m_tilePrefab, transform, false);
        tile.transform.localPosition = m_currentPosition;
        m_currentPosition.x += m_tileSpacing;
        tile.SetActive(true);
        var ac = tile.GetComponent<ArrowCard>();
        ac.m_tweens.ActivationUnveil.StartTween();
        
    }

    private void CreateCards()
    {
        for (int i = 0; i < m_nbCards; ++i)
        {
            for (int col = 0; col < m_nbCards; ++col)
            {
                CreateCard();  
            }
        }
    }

    public FlagDistributor Distributor { get
        {
            return m_refDistributor;
        }
    }

    public override ArrowFlag PickRandomFlags()
    {
        return m_refDistributor.PickRandomFlags();
    }
}
