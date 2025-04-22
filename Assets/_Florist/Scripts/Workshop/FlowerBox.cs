using Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FlowerBox : MonoBehaviour
{
    [SerializeField] private Image _flowerBoxImage;
    [SerializeField] private Image _flowerImage;
    [SerializeField] private TextMeshProUGUI _flowerName;
    private int _index;
    private Sprite _flowerSprite;

    public FlowerBox SetIndex(int index)
    {
        _index = index;
        return this;
    }

    public FlowerBox SetFlowerBoxImage(Sprite flowerSprite)
    {
        _flowerBoxImage.sprite = flowerSprite;
        return this;
    }

    public FlowerBox SetFlowerImage(Sprite flowerSprite)
    {
        _flowerSprite = flowerSprite;
        _flowerImage.sprite = flowerSprite;
        return this;
    }

    public FlowerBox SetFlowerName(string flowerName, FlowerColor flowerColor)
    {
        string localizedFlowerName = LocalizationManager.GetLocalizedText(flowerName);
        if (flowerColor == FlowerColor.None)
        {
            _flowerName.text = $"{localizedFlowerName}";
        }
        else
        {
            string colorName = LocalizationManager.GetLocalizedText(flowerColor.ToString().ToLower());
            _flowerName.text = $"{colorName} {localizedFlowerName}";
        }
        return this;
    }

    public FlowerBox SetTransform(Transform targetTransform)
    {
        transform.SetPositionAndRotation(targetTransform.position, targetTransform.rotation);
        transform.localScale = targetTransform.localScale;
        return this;
    }

    public void OnBoxSelected()
    {
        References.WorkshopPage.OnBoxSelected(_flowerSprite);
    }
}
