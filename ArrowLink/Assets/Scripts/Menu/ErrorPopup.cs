using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ArrowLink
{
    public class ErrorPopup : BasicPopup
    {
        [SerializeField]
        private Text m_message = null;

        public void Show(string message)
        {
            m_message.text = message;
            base.Show();
        }
    }
}
