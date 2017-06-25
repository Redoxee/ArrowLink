using System;
using System.Collections.Generic;
using UnityEngine;

public class SequenceUITween : BaseUITween {

	[SerializeField]
	UITweenList m_tweenList = new UITweenList();

	int currentIndex = -1;

	Action m_endAction = null;

	public override bool isAnimating()
	{
		return currentIndex >= 0;
	}

	public override void StartTween(Action onTweenEnd)
	{
		FireTween();
	}

	void FireTween()
	{
		currentIndex++;
		if (currentIndex >= m_tweenList.Count)
		{
			currentIndex = -1;
			if (m_endAction != null)
				m_endAction();

			return;
		}
		bool isLastTween = currentIndex == m_tweenList.Count - 1;
		bool isBlocing = m_tweenList[currentIndex].IsBlocking;
		Action endStep = null;
		if (isBlocing || isLastTween)
			endStep = FireTween;
		m_tweenList[currentIndex].TweenToPlay.StartTween(endStep);
		if (!isBlocing)
			FireTween();
	}

	[Serializable]
	public class UITweenList : List<TweenSequenceEntry> { } 

	[Serializable]
	public class TweenSequenceEntry
	{
		public bool IsBlocking = true;

		public SingleUITween TweenToPlay = null;
	}

}