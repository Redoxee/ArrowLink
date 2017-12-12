using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

namespace ArrowLink
{
    public class MainSceneInstanciator : MonoBehaviour {

        private void Start()
        {
            MainProcess.InvokMainSceneIfNecessary();
            Destroy(this);
        }
    }
}
