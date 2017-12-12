using System.Collections;
using System.Collections.Generic;
using Facebook.Unity;
using UnityEngine.Analytics;
using UnityEngine;

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
            FB.LogAppEvent(eventName, numValue ,parameters);
            Analytics.CustomEvent(eventName, parameters);
        }
    }
}