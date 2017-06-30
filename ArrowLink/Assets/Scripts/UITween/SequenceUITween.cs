using System;
using System.Collections.Generic;
using UnityEngine;

public class SequenceUITween : BaseUITween {

	[SerializeField]
	TweenSequenceEntry[] m_tweenList = null;

	int currentIndex = -1;

	Action m_endAction = null;

	public override bool isAnimating()
	{
		return currentIndex >= 0;
	}

	public override void StartTween(Action onTweenEnd)
	{
		m_endAction = onTweenEnd;
		FireTween();
	}

	void FireTween()
	{
		currentIndex++;
		if (currentIndex >= m_tweenList.Length)
		{
			currentIndex = -1;
			if (m_endAction != null)
				m_endAction();

			return;
		}
		bool isLastTween = currentIndex == m_tweenList.Length - 1;
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


	public override UITweenParameters[] GetParameters ()
	{
		UITweenParameters[] parameters = new UITweenParameters[0];
		int currentLength = 0;
		foreach(TweenSequenceEntry entry in m_tweenList)
		{
			
			UITweenParameters[] array2 = entry.TweenToPlay.GetParameters();

			Array.Resize<UITweenParameters>(ref parameters, currentLength + array2.Length);
			Array.Copy(array2, 0, parameters, currentLength, array2.Length);
			currentLength = parameters.Length;
		}
		return parameters;
	}
}