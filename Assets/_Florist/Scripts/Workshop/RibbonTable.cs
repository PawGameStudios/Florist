using System.Collections.Generic;
using Config;
using UnityEngine;
using UnityEngine.UI;

public class RibbonTable : MonoBehaviour
{
    [SerializeField] private List<Image> _ribbonImages;
    [SerializeField] private Sprite _lockSprite;
    [SerializeField] private Transform _paperSitPositionTop_1;
    [SerializeField] private Transform _paperSitPositionTop_2_1;
    [SerializeField] private Transform _paperSitPositionTop_2_2;
    [SerializeField] private Transform _paperSitPositionBottom_1;
    [SerializeField] private Transform _paperSitPositionBottom_2_1;
    [SerializeField] private Transform _paperSitPositionBottom_2_2;

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

    public void AddFlowerToRibbonArea(PaperArea paperArea, int totalOrderCount, int orderIndex)
    {
        Debug.Log($"Adding flowers to ribbon area. Total orders: {totalOrderCount}, Order index: {orderIndex}");

        _paper = paperArea;

        if (_ribbons.Count <= 4)
        {
            if (totalOrderCount > 1)
            {
                if (orderIndex == 0)
                {
                    _paper.transform.SetPositionAndRotation(_paperSitPositionTop_2_1.position, _paperSitPositionTop_2_1.rotation);
                }
                else if (orderIndex == 1)
                {
                    _paper.transform.SetPositionAndRotation(_paperSitPositionTop_2_2.position, _paperSitPositionTop_2_2.rotation);
                }
            }
            else
            {
                _paper.transform.SetPositionAndRotation(_paperSitPositionTop_1.position, _paperSitPositionTop_1.rotation);

                Debug.Log("_paper pos: " + _paper.transform.position);
                Debug.Log("_paperSitPositionTop_1 pos: " + _paperSitPositionTop_1.position);



                // _paper.GetComponent<RectTransform>().anchoredPosition = _paperSitPositionTop_1.GetComponent<RectTransform>().anchoredPosition;
                // _paper.GetComponent<RectTransform>().rotation = _paperSitPositionTop_1.GetComponent<RectTransform>().rotation;
            }
        }
        else
        {
            if (totalOrderCount > 1)
            {
                if (orderIndex == 0)
                {
                    _paper.transform.SetPositionAndRotation(_paperSitPositionBottom_2_1.position, _paperSitPositionBottom_2_1.rotation);
                }
                else if (orderIndex == 1)
                {
                    _paper.transform.SetPositionAndRotation(_paperSitPositionBottom_2_2.position, _paperSitPositionBottom_2_2.rotation);
                }
            }
            else
            {
                Debug.Log("Setting paper position to bottom 1.");
                // _paper.transform.SetPositionAndRotation(_paperSitPositionBottom_1.position, _paperSitPositionBottom_1.rotation);
                _paper.GetComponent<RectTransform>().anchoredPosition = _paperSitPositionBottom_1.GetComponent<RectTransform>().anchoredPosition;
                _paper.GetComponent<RectTransform>().rotation = _paperSitPositionBottom_1.GetComponent<RectTransform>().rotation;
            }
        }
    }

    public void OnRibbonClicked(int index)
    {
        if (index >= _ribbons.Count)
            return;

        var selectedRibbon = _ribbons[index];
        _paper.OnRibbonSelected(selectedRibbon.RibbonType);
    }
}
