namespace Florist.Merge
{
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance { get; private set; }

    [SerializeField] private Canvas mergeCanvas;
    [SerializeField] private Canvas gardenCanvas;
    [SerializeField] private Canvas inventoryCanvas; // Envanter için ayrı canvas
    [SerializeField] private GameManager gameManager;

    public enum CanvasType
    {
        Merge,
        Garden
    }

    private CanvasType currentCanvas = CanvasType.Merge;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        // Envanter canvas'ını her zaman görünür yap
        if (inventoryCanvas != null) inventoryCanvas.gameObject.SetActive(true);

        // Başlangıçta Merge canvas'ı göster
        ShowCanvas(CanvasType.Merge);
    }

    public void ShowCanvas(CanvasType canvasType)
    {
        if (gameManager != null) gameManager.ClearFeedback();
        // Tüm canvas'ları gizle
        if (mergeCanvas != null) mergeCanvas.gameObject.SetActive(false);
        if (gardenCanvas != null) gardenCanvas.gameObject.SetActive(false);

        // İstenen canvas'ı göster
        switch (canvasType)
        {
            case CanvasType.Merge:
                if (mergeCanvas != null) mergeCanvas.gameObject.SetActive(true);
                break;
            case CanvasType.Garden:
                if (gardenCanvas != null) gardenCanvas.gameObject.SetActive(true);
                break;
        }

        // Envanter canvas'ı her zaman görünür kalır
        if (inventoryCanvas != null) inventoryCanvas.gameObject.SetActive(true);

        currentCanvas = canvasType;
        Debug.Log($"Canvas değiştirildi: {canvasType}");
    }

    public void SwitchToMergeCanvas()
    {
        ShowCanvas(CanvasType.Merge);
    }

    public void SwitchToGardenCanvas()
    {
        ShowCanvas(CanvasType.Garden);
    }

    public void ToggleCanvas()
    {
        if (currentCanvas == CanvasType.Merge)
        {
            ShowCanvas(CanvasType.Garden);
        }
        else
        {
            ShowCanvas(CanvasType.Merge);
        }
    }

    public CanvasType GetCurrentCanvas()
    {
        return currentCanvas;
    }

    public bool IsMergeCanvasActive()
    {
        return currentCanvas == CanvasType.Merge;
    }

    public bool IsGardenCanvasActive()
    {
        return currentCanvas == CanvasType.Garden;
    }
}

}
