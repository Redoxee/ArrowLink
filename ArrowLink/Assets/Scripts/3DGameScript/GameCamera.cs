using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	[RequireComponent(typeof(Camera))]
	public class GameCamera : MonoBehaviour
	{
		const float TargetWidth = 16f / 2f;
		const float HalfBoardSize = 12.5f / 2f;
		const float BoardPadding = .25f;
		const float PlayableHalfHeight = 2f;
		const float PlayablePadding = .25f;


		[SerializeField]
		GameObject m_boardRef = null;

		[SerializeField]
		GameObject m_palyableTileAnchor = null;

		public void Initialize()
		{
			Debug.Assert(m_boardRef != null);

			var camera = GetComponent<Camera>();
			var ratio = (float)Screen.height / (float)Screen.width;
			var halfHeight = (TargetWidth) * ratio;
			camera.orthographicSize = halfHeight;
            var camPos = camera.transform.position;
            camera.transform.position = new Vector3(camPos.x, halfHeight, camPos.z);
			//var boardPos = m_boardRef.transform.position;
			//boardPos.x = transform.position.x;
			//boardPos.y = -halfHeight + HalfBoardSize + BoardPadding;
			//m_boardRef.transform.position = boardPos;
            
		}
	}
}
