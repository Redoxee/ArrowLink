//#define AMG_UNLOCK_GAME
//#define AMG_CANCELABLE_IAP
//#define AMG_CANCELABLE_IAP


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace ArrowLink
{
    public class MonetManager : IStoreListener {
        const int c_nb_game_at_start = 15;
        const int c_nb_game_per_day = 2;

        const string c_game_key = "monet_game";
        const string c_main_sku = "com.antonmakesgames.arrowlink.gameunlock";

        const string c_purchased_save_key = "game_unlocked";

        private int m_nbGame = 0;
        public int NbGame { get { return Mathf.Max(m_nbGame, 0); } }

        private Action m_purchaseSuccess = null;
        private Action m_purchaseFailed = null;

        public HashSet<Action> InitializedListeners = new HashSet<Action>();

        public void Initialize(bool isNewDay)
        {
            InitializeIAP();

            if (PlayerPrefs.HasKey(c_game_key))
            {
                m_nbGame = PlayerPrefs.GetInt(c_game_key);
                if (isNewDay)
                    m_nbGame++;
            }
            else
            {
                m_nbGame = c_nb_game_at_start;
            }

            if (PlayerPrefs.HasKey(c_purchased_save_key))
            {
                m_isGameUnlocked = PlayerPrefs.GetInt(c_purchased_save_key) > 0;
            }

            Save();

#if AMG_UNLOCK_GAME
        m_isGameUnlocked = true;
#endif
        }

        #region IAP

        private static IStoreController m_storeController;          // The Unity Purchasing system.
        private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

        private void InitializeIAP()
        {// If we have already connected to Purchasing ...
            if (IsInitialized())
            {
                return;
            }

            var module = StandardPurchasingModule.Instance();
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
            ProductType productType = ProductType.NonConsumable;
#if AMG_CANCELABLE_IAP
            productType = ProductType.Consumable;
#endif

            builder.AddProduct(c_main_sku, productType);
			UnityPurchasing.Initialize(this,builder);
        }

        private bool IsInitialized()
        {
            return m_storeController != null && m_StoreExtensionProvider != null;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
            MainProcess.Instance.ShowErrorMessage("Error on initialization :'( \n" + error);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("OnInitialized: PASS");

            m_storeController = controller;
            m_StoreExtensionProvider = extensions;

            var product = controller.products.WithID(c_main_sku);
            if (product.hasReceipt)
            {
                if (!m_isGameUnlocked)
                {
                    m_isGameUnlocked = true;
                    PlayerPrefs.SetInt(c_purchased_save_key, 1);
                    PlayerPrefs.Save();

                    foreach (var listener in InitializedListeners)
                    {
                        if (listener != null)
                        {
                            listener();
                        }
                    }
                }
            }
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {

            bool validPurchase = true; // Presume valid for platforms with no R.V.
            Debug.LogFormat("Purchase success : {0}", e);
            /*
            // Unity IAP's validation logic is only included on these platforms.
#if UNITY_ANDROID || UNITY_IOS
            // Prepare the validator with the secrets we prepared in the Editor
            // obfuscation window.
            var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
                AppleTangle.Data(), Application.identifier);
            try
            {
                // On Google Play, result has a single product ID.
                // On Apple stores, receipts contain multiple products.
                var result = validator.Validate(e.purchasedProduct.receipt);
                // For informational purposes, we list the receipt(s)
                Debug.Log("Receipt is valid. Contents:");
                foreach (IPurchaseReceipt productReceipt in result)
                {
                    Debug.Log(productReceipt.productID);
                    Debug.Log(productReceipt.purchaseDate);
                    Debug.Log(productReceipt.transactionID);
                }
            }
            catch (IAPSecurityException exception)
            {
                Debug.LogFormat("Invalid receipt, not unlocking content : {0}", exception);
                validPurchase = false;
            }
#endif
*/
            if (validPurchase)
            {
                {
                    m_isGameUnlocked = true;
                    PlayerPrefs.SetInt(c_purchased_save_key, 1);
                    PlayerPrefs.Save();
                    if (m_purchaseSuccess != null)
                    {
                        m_purchaseSuccess();
                    }
                }
            }
            else
            {
                Debug.Log("Purchase non valide :(");
                MainProcess.Instance.ShowErrorMessage("Something went wrong sorry :(");
            }


            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product i, PurchaseFailureReason p)
        {
            MainProcess.Instance.ShowErrorMessage("SomeThing went wrong : \n" + p.ToString());
            if (m_purchaseFailed != null)
            {
                m_purchaseFailed();
            }
        }

        public static bool RestorePurchaseAllowed
        {
            get
            {
#if UNITY_IOS
                return true;
#else
                return false;
#endif
            }
        }

        public static bool CancelIAPAllowed
        {
            get {
#if AMG_CANCELABLE_IAP
                return true;
#else
                return false;
#endif
            }
        }

        public void CancelIAPPurchase()
        {
#if AMG_CANCELABLE_IAP
			{
				var product = m_storeController.products.WithID(c_main_sku);
                m_storeController.ConfirmPendingPurchase(product);

                m_isGameUnlocked = false;
                PlayerPrefs.SetInt(c_purchased_save_key, -1);
			}
#endif
        }

        public void RestorePurchases(Action onSuccess)
        {
            if (!IsInitialized())
            {
                Debug.Log("RestorePurchases FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                m_purchaseSuccess = onSuccess;
                Debug.Log("RestorePurchases started ...");

                IAppleExtensions apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
                apple.RestoreTransactions((result) =>
                {
                    Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                });
            }
            else
            {
                Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        #endregion

        public bool ConsumeTry()
        {
            if (m_isGameUnlocked)
                return true;

            if (m_nbGame <= 0)
                return false;
            m_nbGame--;
            Save();
            return true;
        }

        public void Save()
        {
            PlayerPrefs.SetInt(c_game_key, m_nbGame);
            PlayerPrefs.Save();
        }

        public void UnlockFullGame(Action onPurchaseComplete, Action onPurchaseError)
        {
            if (IsInitialized())
            {
                m_purchaseSuccess = onPurchaseComplete;
                m_purchaseFailed = onPurchaseError;
                m_storeController.InitiatePurchase(c_main_sku);
            }
            else
            {
                MainProcess.Instance.ShowErrorMessage("Sorry,, something is not working :(");
                if (onPurchaseError != null)
                {
                    onPurchaseError();
                }
            }
        }

        private bool m_isGameUnlocked = false;
        public bool IsGameUnlocked
        {
            get { return m_isGameUnlocked; }
        }
    }
}