using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowLink
{
    public class OverlinkGUICapsule : MonoBehaviour
    {
        [SerializeField]
        public SingleUITween FlashTween = null;

        [SerializeField]
        public Text DotBonus = null;

        [SerializeField]
        public Text ScoreBonus = null;
    }
}