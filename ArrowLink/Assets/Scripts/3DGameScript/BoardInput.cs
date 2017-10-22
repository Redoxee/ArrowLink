using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ArrowLink
{
	public class BoardInput : MonoBehaviour {

		[SerializeField]
		public BoardSlot[] m_slots;

		GameProcess m_gameProcess = null;

        [SerializeField]
        private GameObject m_buttonsHolder = null;

		public void Initialize(GameProcess gp)
		{
			m_gameProcess = gp;
            int nbSlot = BoardLogic.c_col * BoardLogic.c_col;
            EventTrigger.Entry entry;

            for (int i = 0; i < nbSlot; ++i)
            {
                Transform btn = m_buttonsHolder.transform.GetChild(i);
                int index = i;

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.Drag;
                entry.callback.AddListener((data) => { _OnTileDrag(index, (PointerEventData)data); });
                var evnt = btn.GetComponent<EventTrigger>();
                evnt.triggers.Add(entry);

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener((data) => { OnTilePressed(index); });
                evnt.triggers.Add(entry);

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((data) => { OnTileIn(index); });
                evnt.triggers.Add(entry);

                entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerExit;
                entry.callback.AddListener((data) => { OnTileOut(index); });
                evnt.triggers.Add(entry);
            }

			for (int i = 0; i < m_slots.Length; ++i)
			{
				m_slots[i].X = i % BoardLogic.c_col;
				m_slots[i].Y = i / BoardLogic.c_col;
			}
		}

        private void _OnTileDrag(int index , PointerEventData data)
        {

            //Vector2 dragVector = data.position - data.pressPosition;
            //Vector2 direction = dragVector.normalized;
            //float fangle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg ;
            //int iangle = (int)fangle + 180;
            //iangle += 90;
            //iangle += 45;
            //iangle %= 360;
            //int quadrant = iangle / 45;
            //Debug.LogFormat("angle {0}", quadrant);
            //Camera main = Camera.main;
            //Debug.DrawLine(main.ScreenToWorldPoint(data.pressPosition), main.ScreenToWorldPoint(data.position));
        }

		public void OnTilePressed(int tileIndex)
		{
			m_gameProcess.OnTilePressed(m_slots[tileIndex]);
		}

		public void OnTileIn(int index)
		{
			m_slots[index].OnFocus();
		}

		public void OnTileOut(int index)
		{
			m_slots[index].OnUnfocus();
		}
	}
}
