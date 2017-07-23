using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour {

	[SerializeField]
	Text m_scoreText = null;

	private void Start()
	{
		m_scoreText.text ="0";
	}

	public void OnPausePressed()
	{
		UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(0);
	}

	public void NotifyScoreChanged(int newScore, int scoreDelta)
	{
		m_scoreText.text = newScore.ToString();
	}
}
