using System.Collections.Generic;
using Config;
using UnityEngine;
using UnityEngine.UI;

public class RibbonTable : MonoBehaviour
{
    [SerializeField] private List<Image> _ribbonImages;
    [SerializeField] private Sprite _lockSprite;
    [SerializeField] private Transform _paperSitPosition1;
    [SerializeField] private Transform _paperSitPosition2;
    private List<RibbonInfo> _ribbons = new();
    private PaperArea _paper = new();

    public void Initialize(List<RibbonInfo> ribbons)
    {
        _ribbons = ribbons;

        int i = 0;
        for (; i < ribbons.Count; i++)
        {
            if (i >= _ribbonImages.Count)
                break;

            _ribbonImages[i].sprite = ribbons[i].Sprite;
            _ribbonImages[i].gameObject.SetActive(true);
        }
        for (; i < _ribbonImages.Count; i++)
        {
            _ribbonImages[i].sprite = _lockSprite;
            _ribbonImages[i].gameObject.SetActive(true);
        }
    }

    public void AddFlowerToRibbonArea(PaperArea paperArea)
    {
        _paper = paperArea;

        if (_ribbons.Count > 4)
            _paper.transform.SetPositionAndRotation(_paperSitPosition1.position, _paperSitPosition1.rotation);
        else
            _paper.transform.SetPositionAndRotation(_paperSitPosition2.position, _paperSitPosition2.rotation);
    }

    public void OnRibbonClicked(int index)
    {
        if (index >= _ribbons.Count)
            return;

        var selectedRibbon = _ribbons[index];
        _paper.OnRibbonSelected(selectedRibbon.RibbonType);
    }
}
