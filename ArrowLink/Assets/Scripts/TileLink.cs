using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileLink : MonoBehaviour {
	readonly float c_diagonalStretch = 1.41421356237f; // sqrt(2)

	public enum LinkOrientation
	{
		Vertical,
		Slash,
		Horizontal,
		BackSlach,
	}

	public void SetOientation(LinkOrientation orientation)
	{
		float angle = 0;
		float stretch = 1;

		switch (orientation)
		{
			case LinkOrientation.Vertical:
				break;
			case LinkOrientation.Slash:
				angle = 45;
				stretch = c_diagonalStretch;
				break;
			case LinkOrientation.Horizontal:
				angle = 90;
				break;
			case LinkOrientation.BackSlach:
				angle = -45;
				stretch = c_diagonalStretch;
				break;
		}

		var tr = transform;
		tr.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
		tr.localScale = new Vector3(1, 1, stretch);
	}
}
