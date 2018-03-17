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
                Achievement achievement = new Achievement(this);
                achievement.EventToWatch = eventToWatch;
                achievement.TargetValue = targetValue;
                achievement.Title = string.Format(title,targetValue);
                achievement.SubTitle = subtitle;
                if (prevId == id)
                {
                    achievement.PrevAchievement = prevAchievement;
                }
                prevId = id;
                prevAchievement = achievement;
                achievement.Index = i;
                m_allAchievement.Add(achievement);
                if (GetEventValue(eventToWatch) < targetValue)
                {
                    m_nonCompletedAchievement.Add(achievement);
                }
            }
        }

        #endregion

        public void _NotifyEventIncrement(string eventName, int Value = 1)
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

        public void _NotifyEventMaxing(string eventName, int Value)
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

        public static void NotifyEventIncrement(string eventName, int value = 1)
        {
            ArrowLink.MainProcess.Instance.Achievements._NotifyEventIncrement(eventName, value);
        }

        public static void NotifyEventMaxing(string eventName, int value)
        {
            ArrowLink.MainProcess.Instance.Achievements._NotifyEventMaxing(eventName, value = 1);
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

        public List<Achievement> GetSortedAchievements()
        {
            m_allAchievement.Sort(SortAchievement);
            return m_allAchievement;
        }

        private static int SortAchievement(Achievement a, Achievement b)
        {
            if (a == b)
                return 0;
            bool aPrev = a.PrevAchievementCompleted;
            bool bPrev = b.PrevAchievementCompleted;
            if (aPrev != bPrev)
            {
                if (aPrev)
                {
                    return 1;
                }
                return -1;
            }
            bool aComp = a.IsComplet;
            bool bComp = b.IsComplet;
            if (aComp != bComp)
            {
                if (aComp)
                {
                    return -1;
                }
                return 1;
            }

            return a.Index - b.Index;
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
        public int Index;
        public Achievement PrevAchievement;
        public int TargetValue ;
        public string EventToWatch ;
        public string Title;
        public string SubTitle;

        private AchievementManager m_managerRef;

        public Achievement(AchievementManager manager)
        {
            m_managerRef = manager;
        }

        public int CurrentValue
        {
            get { return m_managerRef.GetEventValue(EventToWatch); }
        }

        public bool IsComplet
        { get
            {
                return m_managerRef.GetEventValue(EventToWatch) >= TargetValue;
            }
        }

        public bool PrevAchievementCompleted
        {
            get
            {
                return (PrevAchievement == null) || (PrevAchievement.PrevAchievementCompleted && PrevAchievement.IsComplet);
            }
        }

    }
}