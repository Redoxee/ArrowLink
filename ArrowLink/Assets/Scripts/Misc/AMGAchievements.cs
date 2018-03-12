using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AntonMakesGames
{
    public class AchievementManager 
    {
        [Serializable]
        private class EventDictionary : SerializableDictionary<string, int>
        { public EventDictionary() { dictionary = new Dictionary<string, int>(); } }

        private EventDictionary m_events;

        private List<Achievement> m_allAchievement;
        private List<Achievement> m_nonCompletedAchievement;

        public AchievementManager(TextAsset achievementConfiguration)
        {
            Load();
            ParseAchievementConfiguration(achievementConfiguration);
        }

        #region Achivement configuration

        private void ParseAchievementConfiguration(TextAsset achievementConfiguration)
        {
            m_allAchievement = new List<Achievement>();
            m_nonCompletedAchievement = new List<Achievement>();


            string str = achievementConfiguration.text;
            string[] lines = str.Split('\n');
            string prevId = null;
            Achievement prevAchievement = null;
            for (int i = 1; i < lines.Length; ++i)
            {
                string[] columns = lines[i].Split(',');
                string id = columns[0];
                string eventToWatch = columns[1];
                int targetValue = int.Parse(columns[2]);
                string title = columns[3];
                string subtitle = columns[4];
                Achievement achievement = new Achievement();
                achievement.EventToWatch = eventToWatch;
                achievement.TargetValue = targetValue;
                achievement.Title = string.Format(title,targetValue);
                if (prevId == id)
                {
                    achievement.PrevAchievement = prevAchievement;
                }
                prevId = id;
                prevAchievement = achievement;
                m_allAchievement.Add(achievement);
                if (GetEventValue(eventToWatch) < targetValue)
                {
                    m_nonCompletedAchievement.Add(achievement);
                }
            }
        }

        #endregion

        public void NotifyEventIncrement(string eventName, int Value = 1)
        {
            if (m_events.ContainsKey(eventName))
            {
                m_events[eventName] += Value;
            }
            else
            {
                m_events[eventName] = Value;
            }
        }

        public void NotifyEventMaxing(string eventName, int Value)
        {

            if (m_events.ContainsKey(eventName))
            {
                m_events[eventName] = Mathf.Max(Value, m_events[eventName]);
            }
            else
            {
                m_events[eventName] = Value;
            }
        }

        public int GetEventValue(string eventName)
        {
            if (m_events.ContainsKey(eventName))
                return m_events[eventName];
            else
                return 0;
        }

        public List<Achievement> CheckNonCompletedAchievement()
        {
            List<Achievement> result = new List<Achievement>(5);
            int count = m_nonCompletedAchievement.Count;
            for (int i = count - 1; i > -1; --i)
            {
                Achievement ach = m_nonCompletedAchievement[i];
                if (ach.TargetValue <= GetEventValue(ach.EventToWatch))
                {
                    result.Add(ach);
                    m_nonCompletedAchievement.RemoveAt(i);
                    NotifyEventIncrement("AchievementCompleted", 1);
                }
            }
            if(result.Count > 0)
            {
                result.AddRange(CheckNonCompletedAchievement());
            }

            return result;
        }
        #region Save

        public const string c_eventSaveName = "AMG.TrackedEvents";
        public void Save()
        {
            string sevents = JsonUtility.ToJson(m_events);
            PlayerPrefs.SetString(c_eventSaveName,sevents);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            string sevents = PlayerPrefs.GetString(c_eventSaveName, null);
            if (!string.IsNullOrEmpty(sevents))
            {
                m_events = JsonUtility.FromJson<EventDictionary>(sevents);
            }
            else
            {
                m_events = new EventDictionary();
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Save/ResetAchievement")]
        private static void DeleteAchievementSave()
        {
            PlayerPrefs.SetString(c_eventSaveName, null);
            PlayerPrefs.Save();
        }
#endif

        #endregion
    }

    public class Achievement
    {
        public Achievement PrevAchievement;
        public int TargetValue ;
        public string EventToWatch ;
        public string Title;
        public string SubTitle;
    }
}