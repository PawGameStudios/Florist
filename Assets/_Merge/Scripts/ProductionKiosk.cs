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

    [SerializeField] private Transform requirementsContainer;
    [SerializeField] private RequirementItem requirementItemPrefab;
    [SerializeField] private TextMeshProUGUI feedbackText;
    private readonly System.Collections.Generic.List<RequirementItem> requirements = new System.Collections.Generic.List<RequirementItem>();
    private ProductionJob displayedJob;
    private int displayedStage = -1;
    private Tween feedbackTween;
    private Vector2 feedbackPosition;
    private RecipeModal currentModal;
    [SerializeField] private ProductionManager productionManager;
    [SerializeField] private InventoryManager inventoryManager;
    private ProductionJob currentProductionJob;
    private Tween readyTween;
    private Vector3 collectScale;
    private bool wasReady;

    private void Awake()
    {
        if (feedbackText != null) { feedbackPosition = feedbackText.rectTransform.anchoredPosition; feedbackText.gameObject.SetActive(false); }
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
        if (inventoryManager != null) inventoryManager.OnInventoryChanged += UpdateRequirements;
        UpdateKioskVisual();
    }

    public void OnKioskClicked()
    {
        // Only empty plots open the planting picker. The flower itself has no action.
        if (currentProductionJob == null && currentModal == null) OpenRecipeModal();
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
        if (productionManager == null || currentProductionJob == null) return;
        if (currentProductionJob.IsReadyToCollect) productionManager.CollectProduction(currentProductionJob);
        else if (currentProductionJob.UsesGrowthStages && !productionManager.AdvanceGrowth(currentProductionJob))
            ShowMissingMaterials();
    }

    private void ShowMissingMaterials()
    {
        if (feedbackText == null) return;
        feedbackTween?.Kill();
        feedbackText.rectTransform.anchoredPosition = feedbackPosition;
        feedbackText.text = MergeLocalization.Text("merge_not_enough_materials");
        feedbackText.alpha = 0f;
        feedbackText.gameObject.SetActive(true);
        feedbackTween = DOTween.Sequence().SetUpdate(true)
            .Append(feedbackText.DOFade(1f, .15f))
            .Join(feedbackText.rectTransform.DOAnchorPosY(feedbackPosition.y + 24f, .8f))
            .AppendInterval(.45f).Append(feedbackText.DOFade(0f, .25f))
            .OnComplete(() => feedbackText.gameObject.SetActive(false));
    }

    private void UpdateRequirements()
    {
        if (requirementsContainer == null) return;
        if (displayedJob != currentProductionJob || displayedStage != (currentProductionJob != null ? currentProductionJob.growthStage : -1))
        {
            foreach (var item in requirements) if (item != null) { item.gameObject.SetActive(false); Destroy(item.gameObject); }
            requirements.Clear();
            displayedJob = currentProductionJob;
            displayedStage = currentProductionJob != null ? currentProductionJob.growthStage : -1;
            var ingredients = currentProductionJob != null ? currentProductionJob.CurrentIngredients : null;
            if (ingredients != null && requirementItemPrefab != null)
                foreach (var ingredient in ingredients)
                {
                    var item = Instantiate(requirementItemPrefab, requirementsContainer);
                    item.Initialize(ingredient, productionManager, inventoryManager);
                    requirements.Add(item);
                }
        }
        requirementsContainer.gameObject.SetActive(requirements.Count > 0);
        foreach (var item in requirements) item.UpdateStatus();
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
                UpdateRequirements();
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
                UpdateRequirements();
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
        int stage = productionJob.UsesGrowthStages ? productionJob.growthStage : Mathf.FloorToInt(progress * 3);
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
            collectButton.gameObject.SetActive(ready || productionJob.UsesGrowthStages);
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
            timeText.text = productionJob.UsesGrowthStages ? (productionJob.IsReadyToCollect ? MergeLocalization.Text("merge_collect") : MergeLocalization.Format("merge_grow_stage", productionJob.growthStage + 1, 3)) : productionJob.GetRemainingTime();
        }
    }

    private void OnDisable()
    {
        feedbackTween?.Kill();
        if (feedbackText != null) feedbackText.gameObject.SetActive(false);
        if (inventoryManager != null) inventoryManager.OnInventoryChanged -= UpdateRequirements;
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
