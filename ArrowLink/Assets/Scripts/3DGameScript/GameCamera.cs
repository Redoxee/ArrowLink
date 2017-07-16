using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArrowLink
{
	[RequireComponent(typeof(Camera))]
	public class GameCamera : MonoBehaviour
	{
		const float TargetWidth = 15f / 2f;
		const float HalfBoardSize = 12.5f / 2f;
		const float BoardPadding = 1.25f;
		const float PlayableHalfHeight = 2f;
		const float PlayablePadding = .5f;


		[SerializeField]
		GameObject m_boardRef = null;

		[SerializeField]
		GameObject m_palyableTileAnchor = null;

		private void Start()
		{
			Debug.Assert(m_boardRef != null);

			var camera = GetComponent<Camera>();
			var ratio = (float)Screen.height / (float)Screen.width;
			var halfHeight = (TargetWidth) * ratio;
			camera.orthographicSize = halfHeight;

			var boardPos = m_boardRef.transform.position;
			boardPos.x = transform.position.x;
			boardPos.y = -halfHeight + HalfBoardSize + BoardPadding;
			m_boardRef.transform.position = boardPos;

			var playable = m_palyableTileAnchor.transform.position;
			playable.x = transform.position.y;
			playable.y = halfHeight - PlayableHalfHeight - PlayablePadding;
			m_palyableTileAnchor.transform.position = playable;
		}
	}
}
