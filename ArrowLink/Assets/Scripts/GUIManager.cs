using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : MonoBehaviour {

	public void OnPausePressed()
	{
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
	}
}
