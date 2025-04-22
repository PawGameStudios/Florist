using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Config;

public class WorkshopPage : Page
{
    [SerializeField] private WrappingMachine _wrappingMachine;
    [SerializeField] private Image _flowerImage;
    [SerializeField] private List<Transform> _flowerBoxPosRefs;
    [SerializeField] private FlowerBox _flowerBoxPrefab;
    [SerializeField] private PaperArea _paperArea;
    [SerializeField] private PaperBox _paperBox;
    [SerializeField] private Transform _flowerParent;
    [SerializeField] private Transform _flowerBoxParent;
    [SerializeField] private Transform _workshopPanel;
    private readonly List<FlowerBox> _flowerBoxes = new();
    private const float DURATION = .8f;
    private const Ease EASE = Ease.OutBack;
    private bool _isInitialized = false;
    private bool _isInputWaiting = false;
    private List<GameObject> _flowersForBouquet = new();

    void OnEnable()
    {
        Open();
    }

    public override void Close(PageData pageData = null, Action onCompleted = null)
    {
        gameObject.SetActive(false);
    }

    public override void Open(PageData pageData = null, Action onCompleted = null)
    {
        base.Open(pageData, onCompleted);
        gameObject.SetActive(true);
        int screenWidth = Screen.width;
        _workshopPanel.localPosition = new Vector3(screenWidth / 2f, _workshopPanel.localPosition.y, _workshopPanel.localPosition.z);
        _workshopPanel.DOLocalMoveX(-screenWidth / 2f, DURATION).SetEase(EASE);

        _wrappingMachine.OpenMachine();

        SetAvailableFlowers();
        // SetAvailablePapers();
    }

    public void OnBoxSelected(Sprite flowerSprite)
    {
        _isInputWaiting = true;
        _flowerImage.sprite = flowerSprite;
    }

    public void OnPaperSelected(GameObject paperOpenAnimation)
    {
        _paperArea.GetPaperToArea(paperOpenAnimation.transform);
        // TODO: play animation
    }

    public void OnPaperAreaClicked(Vector3 targetPos, Vector3 targetRotation)
    {
        if (!_isInputWaiting)
            return;

        var newFlower = Instantiate(_flowerImage, targetPos, Quaternion.Euler(targetRotation), _flowerParent);
        newFlower.gameObject.SetActive(true);
        _flowersForBouquet.Add(newFlower.gameObject);
    }

    public bool CheckIfInMachineArea(Vector2 pos)
    {
        Rect rect = _wrappingMachine.Rect;

        // Get the left, right, top, and bottom boundaries of the rect
        Vector3 rectPos = _wrappingMachine.transform.position;
        float leftSide = rectPos.x - rect.width / 2;
        float rightSide = rectPos.x + rect.width / 2;
        float topSide = rectPos.y + rect.height / 2;
        float bottomSide = rectPos.y - rect.height / 2;

        // Check to see if the point is in the calculated bounds
        if (pos.x >= leftSide &&
            pos.x <= rightSide &&
            pos.y >= bottomSide &&
            pos.y <= topSide)
        {
            return true;
        }
        return false;
    }

    public void OnFlowerGivenToMachine()
    {
        Debug.Log("Flower given to machine.");
        _wrappingMachine.OpenMachine();
    }

    public void OnFlowerReady()
    {
        List<BouquetModel> bouquetModels = new()
        {
            new BouquetModel
            {
                Flowers = new List<BouquetFlowerInfo>
                {
                    new() { FlowerType = FlowerType.Gypsum, Count = 4, FlowerColor = FlowerColor.None },
                    new() { FlowerType = FlowerType.Eucalyptus, Count = 4, FlowerColor = FlowerColor.None },
                    new() { FlowerType = FlowerType.Daisy, Count = 2, FlowerColor = FlowerColor.White },
                },
                RibbonType = RibbonType.Grid,
                WrappingPaperType = WrappingPaperType.Rainbow,
            }
        };
        OrderInfo orderInfo = new()
        {
            BouquetModels = bouquetModels,
        };
        References.DukkanPage.OnFlowerReady(orderInfo);
        Close();
    }

    private void SetAvailableFlowers()
    {
        if (_isInitialized)
            return;

        List<FlowerInfo> flowerInfos = new();
        List<ShopConfig.ShopItemInfo> flowerItems = Configs.ShopConfig.FlowerItems;
        List<int> purchasedFlowerIndexes = SaveSystem.Inst.ShopData.GetPurchasedItems(ItemType.Flower);
        for (int i = 0; i < purchasedFlowerIndexes.Count; i++)
        {
            int configIndex = purchasedFlowerIndexes[i];
            string id = flowerItems[configIndex].Id;
            for (int k = 0; k < Configs.WorkshopConfig.FlowerInfo.Count; k++)
            {
                if (Configs.WorkshopConfig.FlowerInfo[k].Id == id)
                {
                    flowerInfos.Add(Configs.WorkshopConfig.FlowerInfo[k]);
                    break;
                }
            }
        }
        for (int i = 0; i < flowerInfos.Count; i++)
        {
            FlowerBox flowerBox = Instantiate(_flowerBoxPrefab, _flowerBoxParent)
                                    .SetIndex(i)
                                    .SetTransform(_flowerBoxPosRefs[i])
                                    .SetFlowerBoxImage(flowerInfos[i].FlowerInBoxImage)
                                    .SetFlowerImage(flowerInfos[i].Sprite)
                                    .SetFlowerName(flowerInfos[i].Name, flowerInfos[i].Color);
            _flowerBoxes.Add(flowerBox);
        }
        _isInitialized = true;
    }

    private void SetAvailablePapers()
    {
        List<WrappingPaperInfo> paperInfos = new();
        List<ShopConfig.ShopItemInfo> paperItems = Configs.ShopConfig.WrapperItems;
        List<int> purchasedPaperIndexes = SaveSystem.Inst.ShopData.GetPurchasedItems(ItemType.Wrapper);
        for (int i = 0; i < purchasedPaperIndexes.Count; i++)
        {
            int configIndex = purchasedPaperIndexes[i];
            string id = paperItems[configIndex].Id;
            for (int k = 0; k < Configs.WorkshopConfig.WrappingPaperInfo.Count; k++)
            {
                if (Configs.WorkshopConfig.WrappingPaperInfo[k].Id == id)
                {
                    paperInfos.Add(Configs.WorkshopConfig.WrappingPaperInfo[k]);
                    break;
                }
            }
        }

        _paperBox.Initilize(paperInfos);
    }
}
