using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEditorInternal;
using System;
using System.Diagnostics;

namespace AntonMakesGames.Tools
{

	public class BuildTools : EditorWindow
	{
		private const string DATA_PATH = "Assets/";

		private BuildData m_currentBuildData = null;

		private bool m_IncrementPatch = true;
		private bool m_isDevelopementBuild = false;
		private bool m_androidAutoPush = false;

        private bool m_iosAutoRun = false;

		#region Create build data

		[MenuItem("Tools/Time")]
		static void S_GoToTime()
		{
			EditorApplication.ExecuteMenuItem("Edit/Project Settings/Time");
		}
		const string BUILD_DATA_NAME = "BuildData.Asset";


		static void S_CreateBuildData()
		{
			UnityEngine.Debug.Log("Creating a new build data");
			BuildData lf = ScriptableObject.CreateInstance<BuildData>();
			AssetDatabase.CreateAsset(lf, DATA_PATH + BUILD_DATA_NAME);
            UnityEngine.Debug.Log("Build Data created");
		}


		#endregion

		[MenuItem("AMG/Build Tool")]
		public static void ShowWindow()
		{
			var window = EditorWindow.GetWindow(typeof(BuildTools));
			window.name = "Build Tool";
		}

		void Awake()
		{
            if (PlayerPrefs.HasKey("BuildData"))
            {
                m_currentBuildData = AssetDatabase.LoadAssetAtPath<BuildData>(PlayerPrefs.GetString("BuildData"));
            }

            GetConfIfItExists("BuildFolder", ref m_buildFolder);
            GetConfIfItExists("BuildNameTemplate", ref m_buildnameTemplate);
#if UNITY_IOS
            GetConfIfItExists("IOS_Path", ref IOS_path);
            GetConfIfItExists("IOS_FolderName", ref IOS_folderName);
#endif
		}

        private void GetConfIfItExists(string key, ref string confString)
        {
            if (PlayerPrefs.HasKey(key))
                confString = PlayerPrefs.GetString(key);
        }

        bool m_toolConf = false;
        

		Vector2 m_scrollPosition = new Vector2(0, 0);
		void OnGUI()
		{


			m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);

            m_toolConf = EditorGUILayout.BeginToggleGroup("Configuration", m_toolConf);
            if(m_toolConf)
            {
                

                var prev = m_currentBuildData;
                m_currentBuildData = (BuildData)EditorGUILayout.ObjectField("Build Data",m_currentBuildData, typeof(BuildData), false);
                if (prev != m_currentBuildData && m_currentBuildData != null)
                {
                    string buildDataPath = AssetDatabase.GetAssetPath(m_currentBuildData);
                    PlayerPrefs.SetString("BuildData", buildDataPath);
                    PlayerPrefs.Save();
                }
                if (m_currentBuildData == null)
                {
                    if (GUILayout.Button("Create Build Data"))
                    {
                        S_CreateBuildData();
                    }
                }

                EditorGUILayout.BeginHorizontal();
                m_buildFolder = EditorGUILayout.TextField("Build folder", m_buildFolder);
                if (GUILayout.Button("...", GUILayout.Width(25)))
                {
                    var old = m_buildFolder;
                    var n = EditorUtility.OpenFolderPanel("Build Folder", m_buildFolder, "");
                    if (old != n && n != null && n.Length > 0)
                    {
                        if (n.Substring(n.Length - 1) != "/")
                        {
                            n += "/";
                        }
                        if (System.IO.Directory.Exists(n))
                        {
                            m_buildFolder = n;
                            PlayerPrefs.SetString("BuildFolder", n);
                            this.Repaint();
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                { 
                    var n = EditorGUILayout.TextField("Build name template" , m_buildnameTemplate);
                    if (n != m_buildnameTemplate && n != null && n.Length > 0)
                    {
                        m_buildnameTemplate = n;
                        PlayerPrefs.SetString("BuildNameTemplate", n);
                    }
                }
                EditorGUILayout.EndHorizontal();




                GUILayout.Space(15);
                
            }
            EditorGUILayout.EndToggleGroup();
            

			if (GUILayout.Button("Open Player settings"))
			{
				EditorApplication.ExecuteMenuItem("Edit/Project Settings/Player");
			}

			m_IncrementPatch = GUILayout.Toggle(m_IncrementPatch, "Auto Increment Patch");
			m_isDevelopementBuild = GUILayout.Toggle(m_isDevelopementBuild, "Is Developpement Build");
			m_androidAutoPush = GUILayout.Toggle(m_androidAutoPush, "Android Auto push");
            
			if (m_currentBuildData != null)
			{
				GUILayout.Label("Next Build : " + GetVersionName(), EditorStyles.boldLabel);
			}

			if (GUILayout.Button("Build Android"))
			{
				BuildForAndroid(m_isDevelopementBuild, m_IncrementPatch, m_androidAutoPush);
			}

			if (GUILayout.Button("Push last build Android"))
			{
				PushLastAndroidBuild();
			}

#if UNITY_IOS
            m_iosAutoRun = GUILayout.Toggle(m_iosAutoRun, "IOS Auto run");

			if(GUILayout.Button("Build IOS"))
			{
					BuildForIOS(m_isDevelopementBuild,m_IncrementPatch, m_iosAutoRun);

			}
#endif


            if (GUILayout.Button("Open build folder"))
            {
                Process.Start(m_buildFolder);
            }
			GUILayout.EndScrollView();
		}

#region Set scenes in builds
		void SetSceneInProjects()
		{
            
		}
#endregion

#region Builds

		string m_buildFolder = "../../TileLink/";
		string m_buildnameTemplate = "TileLink_{0:D}_{1:D2}_{2:D3}";
		string m_buildVersionNameTemplate = "{0}.{1}.{2}";
		string[] StandardSetup()
		{
			SetSceneInProjects();

			UnityEditor.PlayerSettings.runInBackground = false;
			EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;
			string[] scenes = new string[buildScenes.Length];
			for (int i = 0; i < buildScenes.Length; ++i)
			{
				scenes[i] = buildScenes[i].path;
			}

			return scenes;
		}
        
		string GetVersionName()
		{
			var bd = m_currentBuildData;
			return string.Format(m_buildnameTemplate, bd.Version, bd.Revision, bd.Patch);
		}

		void IncrementPatch()
		{

            BuildData bd = m_currentBuildData;
			bd.Patch += 1;
			EditorUtility.SetDirty(bd);
			AssetDatabase.SaveAssets();
			m_currentBuildData = bd;

            UnityEditor.PlayerSettings.bundleVersion = m_currentBuildData.GetVersionLabel();
            UnityEditor.PlayerSettings.Android.bundleVersionCode += 1;

			UnityEditor.PlayerSettings.iOS.buildNumber = UnityEditor.PlayerSettings.Android.bundleVersionCode.ToString();


		}

		BuildOptions GetOptions(bool isDevelopementBuild)
		{
			BuildOptions bo = BuildOptions.None;
			if (isDevelopementBuild)
			{
				bo = bo | BuildOptions.Development | BuildOptions.AllowDebugging;
			} 
			return bo;
		}
        
#region Platforms

#region Android
		const string c_androidFolder = "Android/";
		const string c_androidExtension = ".apk";

		private string m_lastBuildName = null;

		public void BuildForAndroid(bool isDebug,bool increment, bool autoRun)
		{
			string buildPath = m_buildFolder + c_androidFolder + GetVersionName();

			if(!Directory.Exists(buildPath))
			{
				Directory.CreateDirectory(buildPath);
			}
			string buildName = buildPath + "/" + GetVersionName() + c_androidExtension;

			string[] scenes = StandardSetup();
			BuildOptions options = GetOptions(isDebug);

			if (autoRun)
			{
				options = options | BuildOptions.AutoRunPlayer;
			}

			UnityEngine.Debug.Log("Android building : " + buildName);

			/* * * * * * * *
			 * Actualbuild *
			 * * * * * * * */
			var buildResult = BuildPipeline.BuildPlayer(scenes, buildName, BuildTarget.Android,options);

            int errorCount = buildResult.summary.totalErrors;
            
            var buildString = buildResult.ToString();
			if (errorCount == 0)
			{
				UnityEngine.Debug.Log("Android build complete : " + buildName);
				m_lastBuildName = GetVersionName();

				if (increment)
				{
					IncrementPatch();
				}
			}
			else
			{
				UnityEngine.Debug.LogError("Error building Android: " + errorCount + " errors");
			}
		}

		public void PushLastAndroidBuild()
		{

            string dataPath = Application.dataPath;

			string apkLocation = m_buildFolder + m_lastBuildName + "/" + m_lastBuildName + c_androidExtension;
			if (string.IsNullOrEmpty(apkLocation) || !File.Exists(apkLocation))
			{
				UnityEngine.Debug.LogError("Cannot find .apk file : " + apkLocation );
				return;
			}
			PlayerPrefs.SetString("APK location", apkLocation);

			string adbLocation = PlayerPrefs.GetString("Android debug bridge location");
			if (string.IsNullOrEmpty(apkLocation) || !File.Exists(adbLocation))
				adbLocation = EditorUtility.OpenFilePanel("Android debug bridge", Environment.CurrentDirectory, "exe");
			if (string.IsNullOrEmpty(apkLocation) || !File.Exists(adbLocation))
			{
				UnityEngine.Debug.LogError("Cannot find adb.exe.");
				return;
			}
			PlayerPrefs.SetString("Android debug bridge location", adbLocation);

			ProcessStartInfo info = new ProcessStartInfo
			{
				FileName = adbLocation,
				Arguments = string.Format("install -r \"{0}\"", apkLocation),
				WorkingDirectory = Path.GetDirectoryName(adbLocation),
			};
			Process.Start(info);
		}

#endregion

#region IOS
		
#if UNITY_IOS

		string IOS_path = "/users/antonroy/Desktop/";
		string IOS_folderName = "TileLinkIOS";
		public void BuildForIOS(bool isDebug,bool increment, bool autoRun)
		{

			string buildPath = IOS_path + IOS_folderName + GetVersionName();

			if(!Directory.Exists(buildPath))
			{
				Directory.CreateDirectory(buildPath);
			}
			string buildName = buildPath + "/" + GetVersionName();

			string[] scenes = StandardSetup();
			BuildOptions bo = GetOptions(isDebug);

			if (autoRun)
			{
				bo = bo | BuildOptions.AutoRunPlayer;
			}

			PlayerSettings.bundleVersion = string.Format (m_buildVersionNameTemplate, m_currentBuildData.Version,
				m_currentBuildData.Revision,
				m_currentBuildData.Patch);

			UnityEngine.Debug.Log("IOS building : " + buildName);
			//SetSymbols ();

			var buildResult = BuildPipeline.BuildPlayer(scenes, buildName, BuildTarget.iOS,bo);
            //RestoreSymbols ();

            int nbError = buildResult.summary.totalErrors;
            if (nbError == 0)
			{
				UnityEngine.Debug.Log("IOS build complete : " + buildName);
				m_lastBuildName = GetVersionName();

				if (increment)
				{
					IncrementPatch();
				}
			}
			else
			{
				UnityEngine.Debug.LogError("Error building IOS:\n" + buildResult);
			}		
		}

#endif
#endregion

#region Web
		const string c_webFolder = "Web/";

		public void BuildForWeb(bool isDebug, bool increment)
		{
			string buildPath = m_buildFolder + c_webFolder + GetVersionName();

			if (!Directory.Exists(buildPath))
			{
				Directory.CreateDirectory(buildPath);
			}
			string buildName = buildPath + "/" + GetVersionName();

			string[] scenes = StandardSetup();
			BuildOptions bo = GetOptions(isDebug);

			string buildResult = BuildPipeline.BuildPlayer(scenes, buildName, BuildTarget.WebGL, bo).ToString();

			if (string.IsNullOrEmpty(buildResult))
			{
				UnityEngine.Debug.Log("Web build complete" + buildName);
				if (increment)
				{
					IncrementPatch();
				}
			}
			else
			{
				UnityEngine.Debug.LogError("Error building Web:\n" + buildResult);
			}
		}
#endregion

#endregion

#endregion

#region Show save folder

		[MenuItem("AMG/Open save folder")]
		public static void OpenSaveFolder()
		{
			string itemPath = Application.persistentDataPath;
			itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
			System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
		}

        #endregion

        [MenuItem("AMG/Reset Playerprefs")]
        public static void DeletePlayerPrefs() { PlayerPrefs.DeleteAll(); }
    }
}