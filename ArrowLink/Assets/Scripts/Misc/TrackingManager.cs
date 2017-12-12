using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine.Analytics;
using UnityEngine;
using System.Text;

namespace ArrowLink
{
    public class TrackingManager
    {
        public static void TrackEvent(string eventName, int numValue, Dictionary<string, object> parameters)
        {
            if (MainProcess.IsReady)
                MainProcess.Instance.TrackingManager.TrackEvents(eventName, numValue, parameters);
        }

        public void TrackEvents(string eventName, int numValue, Dictionary<string, object> parameters)
        {
            parameters["GameVersion"] = Application.version;
            if(FB.IsInitialized)
                FB.LogAppEvent(eventName, numValue ,parameters);
            Analytics.CustomEvent(eventName, parameters);

#if UNITY_EDITOR
            StringBuilder builder = new StringBuilder();
            foreach (string key in parameters.Keys)
            {
                builder.Append("[\"").Append(key).Append("\" = ").Append(parameters[key].ToString()).Append("]");
            }
            Debug.LogFormat("{0} | {1} | {2}", eventName, numValue, builder.ToString());
#endif
        }
    }
}