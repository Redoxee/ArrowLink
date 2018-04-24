using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;


namespace ArrowLink
{
    public class AdManager
    {

        #region Ids
#if UNITY_ANDROID
        private const string AdAppId = "ca-app-pub-1214413534015913~1670649138";
#elif UNITY_IOS
        private const string AdAppId = "ca-app-pub-1214413534015913~4773408900";
#else
        private const string AdAppId = "unexpected_platform";
#endif // platforms

#if DEBUG
#if UNITY_ANDROID
        private const string InterstitialId = "unexpected_platform";
        private const string VideoId = "unexpected_platform";
#elif UNITY_IOS
        private const string InterstitialId = "unexpected_platform";
        private const string VideoId = "unexpected_platform";
#else
        private const string InterstitialId = "unexpected_platform";
        private const string VideoId = "unexpected_platform";
#endif // platforms
#else // DEBUG
#if UNITY_ANDROID
        private const string InterstitialId = "unexpected_platform";
        private const string VideoId = "unexpected_platform";
#elif UNITY_IOS
        private const string InterstitialId = "unexpected_platform";
        private const string VideoId = "unexpected_platform";
#else
        private const string InterstitialId = "unexpected_platform";
        private const string VideoId = "unexpected_platform";
#endif // platforms
#endif //DEBUG
        #endregion

        InterstitialAd m_interstitialAd = null;

        public AdManager()
        {
            Initialize();
            refreshInterstitial();
        }

        public void Initialize()
        {
            MobileAds.Initialize(AdAppId);
        }

        private void refreshInterstitial()
        {
            if (m_interstitialAd == null)
            {
                m_interstitialAd = new InterstitialAd(InterstitialId);
                m_interstitialAd.OnAdClosed += HandleOnAdClosed;
                m_interstitialAd.OnAdOpening += HandleOnAdOpened;
                m_interstitialAd.OnAdFailedToLoad += HandleOnAdFailedToLoad;
                m_interstitialAd.OnAdLoaded += HandleOnAdLoaded;
                m_interstitialAd.OnAdLeavingApplication += HandleOnAdLeftApplication;
            }
            else
            {
#if UNITY_IOS
                m_interstitialAd.Destroy();
                m_interstitialAd = new InterstitialAd(InterstitialId);

                m_interstitialAd.OnAdClosed += HandleOnAdClosed;
                m_interstitialAd.OnAdOpening += HandleOnAdOpened;
                m_interstitialAd.OnAdFailedToLoad += HandleOnAdFailedToLoad;
                m_interstitialAd.OnAdLoaded += HandleOnAdLoaded;
                m_interstitialAd.OnAdLeavingApplication += HandleOnAdLeftApplication;
#endif
            }


            AdRequest request = new AdRequest.Builder().Build();
            m_interstitialAd.LoadAd(request);
        }

        public bool IsInterstitialReady
        {
            get
            {
                return m_interstitialAd != null && m_interstitialAd.IsLoaded();
            }
        }

        public void RequestInterstitial()
        {
            if(IsInterstitialReady)
                m_interstitialAd.Show();
        }


        ~AdManager()
        {
            if (m_interstitialAd != null)
            {
                m_interstitialAd.Destroy();
            }
        }
        public void HandleOnAdLoaded(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdLoaded event received");
        }

        public void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs args)
        {
            MonoBehaviour.print("HandleFailedToReceiveAd event received with message: "
                                + args.Message);
        }

        public void HandleOnAdOpened(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdOpened event received");
        }

        public void HandleOnAdClosed(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdClosed event received");
        }

        public void HandleOnAdLeftApplication(object sender, EventArgs args)
        {
            MonoBehaviour.print("HandleAdLeftApplication event received");
        }
    }

}