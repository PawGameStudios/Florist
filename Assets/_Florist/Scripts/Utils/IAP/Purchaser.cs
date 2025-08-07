// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using Unity.Services.Core;
// using Unity.Services.Core.Environments;
// using UnityEngine.Purchasing;
// using UnityEngine.Purchasing.Extension;
// using System.Threading.Tasks;
// using PlayerDuo;

// namespace IAP
// {
//     public class Purchaser : MonoBehaviour, IDetailedStoreListener
//     {
//         public static Purchaser Instance { get; private set; }
//         public bool IsInitialized => m_StoreController != null && m_StoreExtensionProvider != null;
//         public bool HasInitialized { get; private set; }
//         public bool WaitingInitialize { get; private set; }
//         public Action PurchaseRestored, PurchaseFailed, PurchaseStarted, ProductPricesReady;
//         public Action<List<Reward>> PurchaseSuccess;

//         private IStoreController m_StoreController;          // The Unity Purchasing system.
//         private IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.
//         private List<string> _products;
//         private List<Reward> _rewards = new();


//         #region Initialize
//         private void Awake()
//         {
//             if (Instance == null)
//             {
//                 Instance = this;
//                 DontDestroyOnLoad(gameObject);
//             }
//             else
//             {
//                 Destroy(gameObject);
//                 return;
//             }
//         }

//         private void Start()
//         {
//             _ = StartInitializeProducts(Configs.IAPConfig.Products);
//         }

//         /// <summary>
//         /// Start initializing IAP products after IAP Screen is prepared.
//         /// </summary>
//         private async Task StartInitializeProducts(List<string> products)
//         {
//             if (products == null || products.Count == 0) return;

//             try
//             {
//                 var options = new InitializationOptions().SetEnvironmentName("production");
//                 await UnityServices.InitializeAsync(options);
//             }
//             catch (Exception exception)
//             {
//                 // An error occurred during initialization.
//                 FirebaseController.Instance.SendException(exception);
//             }

//             _products = products;

//             if (m_StoreController == null)
//                 InitializePurchasing();
//             else
//                 ProductPricesReady?.Invoke();
//         }

//         private void InitializePurchasing()
//         {
//             if (IsInitialized)
//                 return;

//             WaitingInitialize = true;

//             try
//             {
//                 var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

//                 for (int i = 0; i < _products.Count; i++)
//                     builder.AddProduct(_products[i], ProductType.Consumable);

//                 UnityPurchasing.Initialize(this, builder);
//             }
//             catch (Exception e)
//             {
//                 FirebaseController.Instance.SendException(e);
//             }
//         }

//         /// <summary>
//         /// Called when Unity IAP is ready to make purchases.
//         /// </summary>
//         public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
//         {
//             HasInitialized = true;
//             WaitingInitialize = false;

//             try
//             {
//                 m_StoreController = controller;
//                 m_StoreExtensionProvider = extensions;

//                 ProductPricesReady?.Invoke();
//             }
//             catch (Exception e)
//             {
//                 HasInitialized = false;
//                 FirebaseController.Instance.SendException(e);
//             }
//         }

//         /// <summary>
//         /// Called when Unity IAP encounters an unrecoverable initialization error.
//         ///
//         /// Note that this will not be called if Internet is unavailable; Unity IAP
//         /// will attempt initialization until it becomes available.
//         /// </summary>
//         public void OnInitializeFailed(InitializationFailureReason error)
//         {
//             FirebaseController.Instance.SetCrashlyticsLog($"reason: {error}");
//             FirebaseController.Instance.SendException(new Exception("IAP init failed"));
//             HasInitialized = false;
//             WaitingInitialize = false;
//         }

//         public void OnInitializeFailed(InitializationFailureReason error, string message)
//         {
//             FirebaseController.Instance.SetCrashlyticsLog($"reason: {error}");
//             FirebaseController.Instance.SetCrashlyticsLog($"message: {message}");
//             HasInitialized = false;
//             WaitingInitialize = false;
//         }
//         #endregion


//         #region Price Getters
//         public string GetLocalizedPriceString(string productId)
//         {
//             if (!IsInitialized)
//                 return "Loading...";

//             if (_products.Contains(productId))
//                 return m_StoreController.products.WithID(productId).metadata.localizedPriceString;
//             else
//                 return "Loading...";
//         }

//         public decimal GetLocalizedPrice(string productId)
//         {
//             if (!IsInitialized)
//                 return 0;

//             if (_products.Contains(productId))
//                 return m_StoreController.products.WithID(productId).metadata.localizedPrice;
//             else
//                 return 0;
//         }

//         public string GetLocalizedCurrencySymbol(string productId)
//         {
//             if (!IsInitialized)
//                 return null;

//             if (_products.Contains(productId))
//                 return m_StoreController.products.WithID(productId).metadata.isoCurrencyCode;
//             else
//                 return null;
//         }

//         public bool IsProductAvailable(string productId)
//         {
//             if (!IsInitialized)
//                 return false;

//             if (_products.Contains(productId))
//                 return m_StoreController.products.WithID(productId).availableToPurchase;
//             else
//                 return false;
//         }
//         #endregion


//         #region Purchase
//         public void BuyProduct(string productId, List<Reward> rewardsToPurchase)
//         {
//             _rewards = rewardsToPurchase;
//             if (IsInitialized)
//             {
//                 Product product = m_StoreController.products.WithID(productId);

//                 if (product != null && product.availableToPurchase)
//                 {
//                     PurchaseStarted?.Invoke();
//                     m_StoreController.InitiatePurchase(product);
//                 }
//                 else
//                 {
//                     Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
//                     PurchaseFailed?.Invoke();
//                 }
//             }
//             else
//             {
//                 Debug.Log("BuyProductID FAIL. Not initialized.");
//                 PurchaseFailed?.Invoke();
//             }
//         }

//         /// <summary>
//         /// Called when a purchase completes.
//         ///
//         /// May be called at any time after OnInitialized().
//         /// </summary>
//         public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
//         {
//             string productId = e.purchasedProduct.definition.id;
//             if (Configs.IAPConfig.Products.Contains(productId))
//             {
//                 FirebaseController.Instance.SendCustomEvent($"iap_purchase_{productId}");
//                 PurchaseSuccess?.Invoke(_rewards);
//             }
//             else
//             {
//                 FirebaseController.Instance.SetCrashlyticsLog($"productId: {productId}");
//                 FirebaseController.Instance.SendException(new Exception("IAP purchase failed"));
//                 PurchaseFailed?.Invoke();
//             }
//             return PurchaseProcessingResult.Complete;
//         }

//         /// <summary>
//         /// Called when a purchase fails.
//         /// IStoreListener.OnPurchaseFailed is deprecated,
//         /// use IDetailedStoreListener.OnPurchaseFailed instead.
//         /// </summary>
//         public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
//         {
//             if (failureReason != PurchaseFailureReason.UserCancelled && failureReason != PurchaseFailureReason.PaymentDeclined)
//             {
//                 FirebaseController.Instance.SetCrashlyticsLog($"failureReason: {failureReason}");
//                 FirebaseController.Instance.SetCrashlyticsLog($"product: {product.definition.id}");
//                 FirebaseController.Instance.SendException(new Exception("IAP purchase failed"));
//             }
//             PurchaseFailed?.Invoke();
//         }

//         /// <summary>
//         /// Called when a purchase fails.
//         /// </summary>
//         public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
//         {
//             FirebaseController.Instance.SetCrashlyticsLog($"productId: {failureDescription.productId}");
//             FirebaseController.Instance.SetCrashlyticsLog($"message: {failureDescription.message}");
//             FirebaseController.Instance.SetCrashlyticsLog($"reason: {failureDescription.reason}");
//             FirebaseController.Instance.SendException(new Exception("IAP purchase failed"));

//             PurchaseFailed?.Invoke();
//         }
//         #endregion

//     }
// }
