using UnityEngine;
using UnityEngine.UI;

public class VersionGrabber : MonoBehaviour {

	private void Start()
	{
		Text text = GetComponent <Text>();
		text.text = "Version : " + Application.version;
	}
}
