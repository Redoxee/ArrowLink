using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace AMG.Tools
{
    public class DefineSymbolsHelper : EditorWindow
    {
        [MenuItem("AMG/Define Tool")]
        public static void OpenWindow()
        {
            DefineSymbolsHelper window = (DefineSymbolsHelper)EditorWindow.GetWindow(typeof(DefineSymbolsHelper));
            window.name = "Define Tool";
            window.Load();
            window.Grab();
        }

        [Serializable]
        class DefineEntity
        {
            public string Name;
            public bool IsActive;
        }

        [Serializable]
        class DefineCollection
        {
            public List<DefineEntity> Collection;
        }

        private bool Contains(string s)
        {
            return m_allDefines.Collection.FindLastIndex(
                (DefineEntity e) =>
                {
                    return e.Name == s;
                }
            ) > -1;
        
    }

        DefineCollection m_allDefines = null;

        const string c_save_key = "define_tool";

        private void Save()
        {
            string str = JsonUtility.ToJson(m_allDefines);
            PlayerPrefs.SetString(c_save_key, str);
            PlayerPrefs.Save();
        }

        private void Load()
        {

            if (PlayerPrefs.HasKey(c_save_key))
            {
                string str = PlayerPrefs.GetString(c_save_key);
                m_allDefines = JsonUtility.FromJson<DefineCollection>(str);
            }
            else
            {
                m_allDefines = new DefineCollection();
                m_allDefines.Collection = new List<DefineEntity>();
            }
        }

        private void Grab()
        {
            var currentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string current = PlayerSettings.GetScriptingDefineSymbolsForGroup(currentGroup);
            var strArray = current.Split(';');
            foreach (string s in strArray)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    Add(s);
                }
            }
        }

        private bool Add(string s)
        {
            if (!string.IsNullOrEmpty(s) && !Contains(s))
            {
                m_allDefines.Collection.Add(new DefineEntity
                {
                    Name = s,
                    IsActive = true,
                });
                return true;
            }
            return false;
        }

        private void Set()
        {
            StringBuilder sb = new StringBuilder();
            foreach (DefineEntity def in m_allDefines.Collection)
            {
                if (def.IsActive)
                {
                    sb.Append(def.Name).Append(';');
                }
            }
            var currentGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(currentGroup, sb.ToString());
        }


        private bool m_settings = false;
        private string m_additionalDefine;
        public void OnGUI()
        {
            m_settings = EditorGUILayout.BeginToggleGroup("Settings", m_settings);
            if (m_settings)
            {
                if (GUILayout.Button("Reset"))
                {
                    m_allDefines.Collection.Clear();
                    Grab();
                }

                if (GUILayout.Button("Grab Values"))
                {
                    Grab();
                }

                if (GUILayout.Button("Save"))
                {
                    Save();
                }
                GUILayout.BeginHorizontal();
                {
                    m_additionalDefine = EditorGUILayout.TextField(m_additionalDefine);
                    if (GUILayout.Button("Add",GUILayout.Width(40)))
                    {
                        bool added = Add(m_additionalDefine);
                        if (added)
                        {
                            m_additionalDefine = "";
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndToggleGroup();

            if (m_allDefines.Collection.Count > 0)
            {
                DisplaySymbols();

                if (GUILayout.Button("Apply"))
                {
                    Set();
                    Save();
                }
            }
        }


        private void DisplaySymbols()
        {
            GUILayout.BeginVertical();
            foreach (var def in m_allDefines.Collection)
            {
                def.IsActive = GUILayout.Toggle(def.IsActive, def.Name);
            }
            GUILayout.EndVertical();
        }
    }
}
