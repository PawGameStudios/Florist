namespace Florist.Merge
{
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ProductionKiosk : MonoBehaviour
{
    [SerializeField] private RecipeModal recipeModalPrefab;
    [SerializeField] private Transform modalParent;
    [SerializeField] private Button kioskButton;
    [SerializeField] private Image kioskImage;
    [SerializeField] private Sprite normalKioskSprite; // Normal durumdaki görsel
    [SerializeField] private TextMeshProUGUI timeText; // Kalan süre metni
    [SerializeField] private Button collectButton; // Topla butonu
    [SerializeField] public string kioskId = "Kiosk_1"; // Her kiosk için benzersiz ID

    private RecipeModal currentModal;
    [SerializeField] private ProductionManager productionManager;
    [SerializeField] private InventoryManager inventoryManager;
    private ProductionJob currentProductionJob;
    private Tween readyTween;
    private Vector3 collectScale;
    private bool wasReady;

    private void Awake()
    {
        if (collectButton != null) collectScale = collectButton.transform.localScale;
    }

    private void OnEnable()
    {
        if (kioskButton != null) kioskButton.onClick.AddListener(OnKioskClicked);
        if (collectButton != null) collectButton.onClick.AddListener(OnCollectButtonClicked);
        if (productionManager != null)
        {
            productionManager.OnProductionChanged += UpdateKioskVisual;
            productionManager.OnKioskProductionChanged += OnKioskProductionChanged;
        }
        UpdateKioskVisual();
    }

    public void OnKioskClicked()
    {
        // Eğer üretim tamamlanmış ve toplanabilir durumdaysa, topla butonuna tıkla
        if (currentProductionJob != null && currentProductionJob.IsReadyToCollect)
        {
            OnCollectButtonClicked();
            return;
        }

        // Aksi halde tarif modalını aç
        if (currentModal == null)
        {
            OpenRecipeModal();
        }
    }

    private void OpenRecipeModal()
    {
        if (recipeModalPrefab != null && modalParent != null)
        {
            currentModal = Instantiate(recipeModalPrefab, modalParent);
            currentModal.Initialize(this, productionManager, inventoryManager);
        }
        else
        {
            Debug.LogError("RecipeModal prefab veya modalParent atanmamış!");
        }
    }

    public void CloseModal()
    {
        if (currentModal != null)
        {
            currentModal.gameObject.SetActive(false);
            Destroy(currentModal.gameObject);
            currentModal = null;
        }
    }

    private void OnCollectButtonClicked()
    {
        if (productionManager != null && currentProductionJob != null && currentProductionJob.IsReadyToCollect)
        {
            productionManager.CollectProduction(currentProductionJob);
        }
    }

    private void UpdateKioskVisual()
    {
        if (productionManager != null)
        {
            var productionInThisKiosk = productionManager.GetProductionInKiosk(kioskId);

            if (productionInThisKiosk != null && productionInThisKiosk.recipe != null)
            {
                Sprite productionSprite = GetProductionStageSprite(productionInThisKiosk);
                if (kioskImage != null && productionSprite != null)
                {
                    kioskImage.sprite = productionSprite;
                }

                currentProductionJob = productionInThisKiosk;
                ShowTimeDisplay(productionInThisKiosk);
                UpdateCollectButton(productionInThisKiosk);
            }
            else
            {
                if (kioskImage != null && normalKioskSprite != null)
                {
                    kioskImage.sprite = normalKioskSprite;
                }

                currentProductionJob = null;
                HideTimeDisplay();
                HideCollectButton();
            }
        }
    }

    private void OnKioskProductionChanged(string changedKioskId)
    {
        // Sadece bu kiosk'ın üretimi değiştiyse güncelle
        if (changedKioskId == kioskId)
        {
            UpdateKioskVisual();
        }
    }

    private Sprite GetProductionStageSprite(ProductionJob productionJob)
    {
        var recipe = productionJob.recipe;
        float progress = productionJob.GetProgress();

        // 3 aşamaya böl
        int stage = Mathf.FloorToInt(progress * 3);
        stage = Mathf.Clamp(stage, 0, 2); // 0, 1, 2 aşamaları

        switch (stage)
        {
            case 0:
                return recipe.productionStage1Sprite;
            case 1:
                return recipe.productionStage2Sprite;
            case 2:
                return recipe.productionStage3Sprite;
            default:
                return recipe.productionStage1Sprite;
        }
    }

    private void ShowTimeDisplay(ProductionJob productionJob)
    {
        if (timeText != null)
        {
            timeText.gameObject.SetActive(true);
        }

        UpdateTimeDisplay(productionJob);
    }

    private void HideTimeDisplay()
    {
        if (timeText != null)
        {
            timeText.gameObject.SetActive(false);
        }
    }

    private void UpdateCollectButton(ProductionJob productionJob)
    {
        if (collectButton != null)
        {
            bool ready = productionJob.IsReadyToCollect;
            collectButton.gameObject.SetActive(ready);
            if (ready && !wasReady)
            {
                readyTween?.Kill();
                collectButton.transform.localScale = collectScale;
                readyTween = collectButton.transform.DOPunchScale(collectScale * .06f, .22f, 1, .1f).SetUpdate(true);
            }
            wasReady = ready;
        }
    }

    private void HideCollectButton()
    {
        wasReady = false;
        readyTween?.Kill();
        if (collectButton != null)
        {
            collectButton.transform.localScale = collectScale;
            collectButton.gameObject.SetActive(false);
        }
    }

    private void UpdateTimeDisplay(ProductionJob productionJob)
    {
        if (timeText != null)
        {
            timeText.text = productionJob.GetRemainingTime();
        }
    }

    private void OnDisable()
    {
        readyTween?.Kill();
        if (collectButton != null) collectButton.transform.localScale = collectScale;
        wasReady = false;
        CloseModal();
        Unsubscribe();
    }

    private void Unsubscribe()
    {
        // Event'ten çık
        if (productionManager != null)
        {
            productionManager.OnProductionChanged -= UpdateKioskVisual;
            productionManager.OnKioskProductionChanged -= OnKioskProductionChanged;
        }

        // Listener'ları temizle
        if (kioskButton != null)
        {
            kioskButton.onClick.RemoveListener(OnKioskClicked);
        }

        if (collectButton != null)
        {
            collectButton.onClick.RemoveListener(OnCollectButtonClicked);
        }
    }
}

}
