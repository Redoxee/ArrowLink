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
            m_text.text = MainProcess.Instance.NbTryAvailable.ToString();
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