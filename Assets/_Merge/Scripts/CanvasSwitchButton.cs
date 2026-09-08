namespace Florist.Merge
{
using UnityEngine;
using UnityEngine.UI;

public class CanvasSwitchButton : MonoBehaviour
{
    [SerializeField] private CanvasManager.CanvasType targetCanvas;
    [SerializeField] private Button switchButton;
    [SerializeField] private CanvasManager canvasManager;

    private void OnEnable()
    {
        if (switchButton != null)
        {
            switchButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        if (canvasManager != null)
        {
            canvasManager.ShowCanvas(targetCanvas);
        }
    }

    private void OnDisable()
    {
        if (switchButton != null)
        {
            switchButton.onClick.RemoveListener(OnButtonClicked);
        }
    }
}

}
