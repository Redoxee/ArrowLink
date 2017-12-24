using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class MainMenu : MonoBehaviour {

        private static MainMenu s_instance = null;

        public static MainMenu Instance { get { return s_instance; } }

        [SerializeField]
        FlagDistributor m_flagDistributor = null;

        public FlagDistributor FlagDistributor { get { return m_flagDistributor; } }

        private void Awake()
        {
            Debug.Assert(s_instance == null, "More than one instance of the main menu !!");
            s_instance = this;
        }

        public void RequestPlay()
        {
            MainProcess.Instance.LoadOrReloadGameScene();
        }
    }
}