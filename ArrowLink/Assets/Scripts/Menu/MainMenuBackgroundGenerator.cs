using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class MainMenuBackgroundGenerator : MonoBehaviour
    {

        [SerializeField]
        private GameObject m_tilePrefab = null;
        [SerializeField]
        private float m_tileSpacing = 2f;
        [SerializeField]
        private int m_column = 6;
        [SerializeField]
        private int m_row = 6;

        void Start()
        {
            CreateCards();
        }


        private void CreateCards()
        {
            Vector3 basePosition = new Vector3(
                -m_column * m_tileSpacing / 2f,
                -m_row * m_tileSpacing / 2f,
                0);

            Vector3 currentPosition = basePosition;

            for (int row = 0; row < m_row; ++row)
            {
                for (int col = 0; col < m_column; ++col)
                {
                    var tile = Instantiate(m_tilePrefab, transform, false);
                    tile.transform.localPosition = currentPosition;
                    currentPosition.x += m_tileSpacing;
                    tile.SetActive(true);
                    var ac = tile.GetComponent<ArrowCard>();
                    ac.m_tweens.ActivationUnveil.StartTween();
                }
                currentPosition.y += m_tileSpacing;
                currentPosition.x = basePosition.x;
            }
        }

//#if UNITY_EDITOR
//        [InspectorButtonAttribute("_GenerateTilesFunction")]
//        private bool _GenerateTiles = false;
//        private void _GenerateTilesFunction()
//        {
//        }
//#endif
    }
}
