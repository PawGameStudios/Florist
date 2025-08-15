using System.Collections;
using System.Collections.Generic;
using Config;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class RibbonTable : SerializedMonoBehaviour
{
    public Transform FirstRibbonTransform => _ribbonImages[0].transform;
    [SerializeField] private List<Image> _ribbonImages;
    [SerializeField] private GameObject _ribbonRodBottom;
    [SerializeField] private Sprite _lockSprite;
    [SerializeField] private Transform _paperSitPositionTop_1;
    [SerializeField] private Transform _paperSitPositionTop_2_1;
    [SerializeField] private Transform _paperSitPositionTop_2_2;
    [SerializeField] private Transform _paperSitPositionBottom_1;
    [SerializeField] private Transform _paperSitPositionBottom_2_1;
    [SerializeField] private Transform _paperSitPositionBottom_2_2;
    [SerializeField] private Dictionary<RibbonType, Animator> _ribbonAnimators;
    private const int RIBBON_COUNT_IN_ROW = 4;

    private List<RibbonInfo> _ribbons = new();
    private PaperArea _paper;
    private RibbonType _selectedRibbon;
    private Animator _ribbonAnimator = null;
    private bool _isRibbonUsed = false;

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
            _ribbonImages[i].gameObject.SetActive(false);
        }

        if (_ribbons.Count > RIBBON_COUNT_IN_ROW)
        {
            _ribbonRodBottom.SetActive(true);
        }
        else
        {
            _ribbonRodBottom.SetActive(false);
        }
    }

    public void AddFlowerToRibbonArea(PaperArea paperArea, int totalOrderCount, int orderIndex)
    {
        Debug.Log($"Adding flowers to ribbon area. Total orders: {totalOrderCount}, Order index: {orderIndex}");

        _paper = paperArea;
        _isRibbonUsed = false;

        if (_ribbons.Count <= RIBBON_COUNT_IN_ROW)
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
        {
            return;
        }

        if (_isRibbonUsed)
        {
            return;
        }

        _isRibbonUsed = true;

        HapticsController.PlayMediumHaptic();

        _selectedRibbon = _ribbons[index].RibbonType;
        _ribbonAnimator = Instantiate(_ribbonAnimators[_selectedRibbon], _paper.transform);
        _ribbonAnimator.transform.SetPositionAndRotation(_paper.RibbonPosRef.position, _paper.RibbonPosRef.rotation);

        StartCoroutine(PlayRibbonAnimation());
    }

    private IEnumerator PlayRibbonAnimation()
    {
        yield return new WaitForEndOfFrame();

        _ribbonAnimator.Play("RibbonSelected");

        // get anim duration
        Invoke(nameof(OnRibbonAnimCompleted), 1.5f);
    }

    private void OnRibbonAnimCompleted()
    {
        _paper.OnRibbonSelected(_selectedRibbon, _ribbonAnimator.gameObject);
    }
}
