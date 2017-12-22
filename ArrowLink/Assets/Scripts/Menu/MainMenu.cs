using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
    public class MainMenu : MonoBehaviour {

        public void RequestPlay()
        {
            MainProcess.Instance.LoadOrReloadGameScene();
        }
    }
}