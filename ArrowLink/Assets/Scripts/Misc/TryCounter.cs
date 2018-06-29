using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowLink
{
    public class TryCounter : MonoBehaviour
    {
        [SerializeField]
        private Text m_text = null;

        private IEnumerator Start()
        {
            yield return new MainProcessYield();
            var monetManager = MainProcess.Instance.MonetManager;
            m_text.text = monetManager.NbGame.ToString();
            if (monetManager.IsGameUnlocked)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        public class MainProcessYield : CustomYieldInstruction
        {
            public override bool keepWaiting
            {
                get
                {
                    return MainProcess.Instance == null;
                }
            }
        }
    }
}