using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Config;
using Sirenix.OdinInspector;
using System.Collections;

[Serializable]
public struct TableParams
{
    public int PaperAreaCount;
    public float Width;
}

public class WorkshopPage : Page
{
    private enum State
    {
        None,
        PaperReady,
        AddingFlowers,
    }

    [Title("Flower Table")]
    [SerializeField] private Dictionary<int, TableParams> _flowerTableParamsByFlowerCount;
    [SerializeField] private Transform _flowerHand;
    [SerializeField] private RectTransform _flowerTable;
    [SerializeField] private Transform _flowerBoxParent;
    [SerializeField] private FlowerBox _flowerBoxPrefab;
    [SerializeField] private List<Transform> _flowerBoxPosRefs;

    [Title("Paper Area")]
    [SerializeField] private List<PaperArea> _paperAreas;
    [SerializeField] private PaperBox _paperBox;

    [Title("Ribbon Table")]
    [SerializeField] private RibbonTable _ribbonTable;
    [SerializeField] private RectTransform _ribbonTableRect;

    [Title("Misc")]
    [SerializeField] private RectTransform _scrollContent;
    [SerializeField] private RectTransform _tableContent;
    [SerializeField] private HorizontalLayoutGroup _tableLayout;
    [SerializeField] private RectTransform _trashBin;
    [SerializeField] private Transform _scissor;
    [SerializeField] private Transform _workshopPanel;

    [Title("Machine")]
    [SerializeField] private RectTransform _machineTable;
    [SerializeField] private WrappingMachine _wrappingMachine;

    private readonly List<FlowerBox> _flowerBoxes = new();
    private const float DURATION = .8f;
    private const Ease SCROLL_INIT_EASE = Ease.OutBack;
    private const Ease MACHINE_EASE = Ease.OutQuint;
    private bool _isInitialized = false;
    private bool _isInputWaiting = false;
    private Flower _selectedFlowerPrefab;
    private Tween _scrollTween;
    private bool _canFlowersBeSelected = false;
    private int _unfinishedOrderCount = 0;
    private int _totalOrderCount = 0;
    private List<BouquetModel> _finishedBouquetModels = new();

    void OnEnable()
    {
        Open();
    }

    void OnDisable()
    {
        _scrollTween?.Kill();
    }

    public override void Close(PageParams pageData = null, Action onCompleted = null)
    {
        gameObject.SetActive(false);
    }

    public override void Open(PageParams pageData = null, Action onCompleted = null)
    {
        base.Open(pageData, onCompleted);

        if (pageData != null && pageData.LoadFromSaveData)
        {
            gameObject.SetActive(true);
            int screenWidth = Screen.width;
            _workshopPanel.localPosition = new Vector3(-screenWidth / 2f, _workshopPanel.localPosition.y, _workshopPanel.localPosition.z);

            _scrollTween?.Kill();
            _scrollTween = _workshopPanel.DOLocalMoveX(-screenWidth / 2f, DURATION).SetEase(SCROLL_INIT_EASE).OnComplete(() =>
            {
                _scrollContent.sizeDelta = new(_tableContent.rect.width, _scrollContent.rect.height);
                onCompleted?.Invoke();
            });

            SetAvailableFlowers();
            SetAvailablePapers();
            SetAvailableRibbons();

            _finishedBouquetModels.Clear();

            _isInitialized = true;

            Load(SaveSystem.Inst.SaveData.WorkshopParams);
        }
        else
        {
            gameObject.SetActive(true);
            int screenWidth = Screen.width;
            _workshopPanel.localPosition = new Vector3(screenWidth / 2f + 400, _workshopPanel.localPosition.y, _workshopPanel.localPosition.z);
            _scrollTween?.Kill();
            _scrollTween = _workshopPanel.DOLocalMoveX(-screenWidth / 2f, DURATION).SetEase(SCROLL_INIT_EASE).OnComplete(() =>
            {
                _scrollContent.sizeDelta = new(_tableContent.rect.width, _scrollContent.rect.height);
                _wrappingMachine.OpenMachine();
                onCompleted?.Invoke();
            });

            SetAvailableFlowers();
            SetAvailablePapers();
            SetAvailableRibbons();
            _isInitialized = true;
        }
    }

    public void SetOrderCount(int count)
    {
        _unfinishedOrderCount = count;
        _totalOrderCount = count;
    }

    public void OnBoxSelected(Flower flowerPrefab, int boxIndex)
    {
        if (!_canFlowersBeSelected)
        {
            return;
        }

        _isInputWaiting = true;
        _selectedFlowerPrefab = flowerPrefab;
        _flowerHand.gameObject.SetActive(true);
        _flowerHand.position = _flowerBoxes[boxIndex].transform.position + Vector3.up * 100f;
    }

    public void OnPaperSelected(GameObject paperOpenAnimation, WrappingPaperType paperType)
    {
        for (int i = 0; i < _paperAreas.Count; i++)
        {
            if (_paperAreas[i].IsEmpty)
            {
                HapticsController.PlayButtonHaptic();
                _canFlowersBeSelected = true;

                _paperAreas[i].gameObject.SetActive(true);
                _paperAreas[i].GetPaperToArea(paperOpenAnimation, paperType, i);
                return;
            }
        }
    }

    public void OnScissorClicked()
    {
        for (int i = 0; i < _paperAreas.Count; i++)
        {
            if (_paperAreas[i].CanUseScissor)
            {
                var scissor = Instantiate(_scissor, _flowerTable);
                scissor.gameObject.SetActive(true);
                _paperAreas[i].OnScissorClicked(scissor);
                break;
            }
        }
    }

    public Flower CreateNewFlower(Vector3 targetPos, Vector3 targetRotation, Transform parent)
    {
        if (!_isInputWaiting)
            return null;

        HapticsController.PlayMediumHaptic();

        var newFlower = Instantiate(_selectedFlowerPrefab, targetPos, Quaternion.Euler(targetRotation), parent);
        newFlower.gameObject.SetActive(true);
        return newFlower;
    }

    public void CheckIfPaperAreaInScreenEdge(Vector3 position, float width, PaperArea paperArea)
    {
        float maxX = -3;
        float minX = -_scrollContent.rect.width + Screen.width + 3;
        var scrollPos = _scrollContent.localPosition;
        var paperPos = paperArea.transform.localPosition;

        if (position.x < width / 3f && scrollPos.x < maxX)
        {
            _scrollContent.localPosition = new Vector3(scrollPos.x + 50, scrollPos.y, scrollPos.z);
            paperArea.transform.localPosition = new Vector3(paperPos.x - 50, paperPos.y, paperPos.z);
        }
        else if (position.x > Screen.width - width / 3f && scrollPos.x > minX)
        {
            _scrollContent.localPosition = new Vector3(scrollPos.x - 50, scrollPos.y, scrollPos.z);
            paperArea.transform.localPosition = new Vector3(paperPos.x + 50, paperPos.y, paperPos.z);
        }
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

    public bool CheckIfInTrashArea(Vector2 pos)
    {
        Rect rect = _trashBin.rect;

        // Get the left, right, top, and bottom boundaries of the rect
        Vector3 rectPos = _trashBin.transform.position;
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

    public bool CheckIfInRibbonArea(Vector2 pos)
    {
        Rect rect = _ribbonTableRect.rect;

        // Get the left, right, top, and bottom boundaries of the rect
        Vector3 rectPos = _ribbonTableRect.transform.position;
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

    public void OnFlowerGivenToMachine(PaperArea paperArea)
    {
        Debug.Log("Flower given to machine.");

        var targetPosX = _trashBin.rect.width + _flowerTable.rect.width + _machineTable.rect.width / 2;
        targetPosX += _tableLayout.spacing * 2f;
        targetPosX -= Screen.width / 2f;
        _scrollTween?.Kill();

        HapticsController.PlayLightHaptic();
        _wrappingMachine.TakeBouquet(paperArea);
        _scrollTween = _scrollContent.DOLocalMoveX(-targetPosX, DURATION).SetEase(MACHINE_EASE).OnComplete(() =>
        {
            _wrappingMachine.StartMachine(paperArea);
        });

        CheckCanAddFlowers();
    }

    public void OnFlowerGivenToTrash(PaperArea paperArea)
    {
        Debug.Log("Flower given to trash.");

        HapticsController.PlayMediumHaptic();

        paperArea.gameObject.SetActive(false);

        CheckCanAddFlowers();
    }

    public void OnFlowerGivenToRibbon(PaperArea paperArea)
    {
        Debug.Log("Flower given to ribbon area.");
        var targetPosX = _trashBin.rect.width + _flowerTable.rect.width + _machineTable.rect.width + _ribbonTableRect.rect.width / 2;
        targetPosX += _tableLayout.spacing * 2f;
        targetPosX -= Screen.width / 2f;
        _scrollTween?.Kill();
        _scrollTween = _scrollContent.DOLocalMoveX(-targetPosX, DURATION).SetEase(MACHINE_EASE);
        HapticsController.PlayLightHaptic();

        _ribbonTable.AddFlowerToRibbonArea(paperArea, _totalOrderCount, _unfinishedOrderCount - 1);
    }

    public void OnFlowerReady(BouquetModel bouquetModel, GameObject bouquetObject)
    {
        _finishedBouquetModels.Add(bouquetModel);

        _unfinishedOrderCount--;
        if (_unfinishedOrderCount <= 0)
        {
            OrderInfo orderInfo = new()
            {
                BouquetModels = _finishedBouquetModels,
            };

            References.DukkanPage.OnFlowerReady(orderInfo, bouquetObject);
            Close();
        }
    }

    public void OnBookClicked()
    {

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
                                    .SetFlowerPrefab(flowerInfos[i].Prefab)
                                    .SetFlowerName(flowerInfos[i].Name, flowerInfos[i].Color);
            _flowerBoxes.Add(flowerBox);
        }

        TableParams tableParams = _flowerTableParamsByFlowerCount[flowerInfos.Count];

        _flowerTable.sizeDelta = new Vector2(tableParams.Width, _flowerTable.sizeDelta.y);

        for (int i = 0; i < tableParams.PaperAreaCount; i++)
        {
            if (i < _paperAreas.Count)
            {
                _paperAreas[i].SetAvailable(true);
            }
        }
    }

    private void SetAvailablePapers()
    {
        if (_isInitialized)
            return;

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

        _paperBox.Initialize(paperInfos);
    }

    private void SetAvailableRibbons()
    {
        if (_isInitialized)
            return;

        List<RibbonInfo> ribbonInfos = new();
        List<ShopConfig.ShopItemInfo> ribbonItems = Configs.ShopConfig.RibbonItems;
        List<int> purchasedRibbonIndexes = SaveSystem.Inst.ShopData.GetPurchasedItems(ItemType.Ribbon);
        for (int i = 0; i < purchasedRibbonIndexes.Count; i++)
        {
            int configIndex = purchasedRibbonIndexes[i];
            string id = ribbonItems[configIndex].Id;
            for (int k = 0; k < Configs.WorkshopConfig.RibbonInfo.Count; k++)
            {
                if (Configs.WorkshopConfig.RibbonInfo[k].Id == id)
                {
                    ribbonInfos.Add(Configs.WorkshopConfig.RibbonInfo[k]);
                    break;
                }
            }
        }

        _ribbonTable.Initialize(ribbonInfos);
    }

    private void CheckCanAddFlowers()
    {
        _canFlowersBeSelected = false;
        for (int i = 0; i < _paperAreas.Count; i++)
        {
            if (_paperAreas[i].CanAddFlowers)
            {
                _canFlowersBeSelected = true;
                break;
            }
        }

        if (_canFlowersBeSelected)
        {
            _flowerHand.gameObject.SetActive(false);
            _isInputWaiting = false;
        }
    }


    #region Save Load
    public List<WorkshopParams> GetPaperSaveInfo()
    {
        List<WorkshopParams> workshopParams = new();
        for (int i = 0; i < _paperAreas.Count; i++)
        {
            WorkshopParams workshopParam = new()
            {
                PaperState = _paperAreas[i].PaperState,
                CurrentFlowers = _paperAreas[i].BouquetModel,
                // SelectedPaperIndex = _paperAreas[i].SelectedPaperIndex,
            };
            workshopParams.Add(workshopParam);
        }
        return workshopParams;
    }

    private void Load(List<WorkshopParams> workshopParams)
    {
        Debug.Log("Load");

        _finishedBouquetModels.Clear();

        for (int i = 0; i < workshopParams.Count; i++)
        {
            PaperArea.State paperState = workshopParams[i].PaperState;
            PaperArea paperArea = _paperAreas[i];

            _paperAreas[i].SetState(paperState);

            switch (paperState)
            {
                case PaperArea.State.None:
                    paperArea.gameObject.SetActive(false);
                    paperArea.SetAvailable(false);
                    break;
                case PaperArea.State.WaitingForPaper:
                    paperArea.gameObject.SetActive(false);
                    paperArea.SetAvailable(true);
                    break;
                case PaperArea.State.AddingFlowers:
                case PaperArea.State.ScissorUsed:
                    SetFlowers(paperArea, workshopParams[i]);
                    break;
                case PaperArea.State.ScissorInUse:
                    SetFlowers(paperArea, workshopParams[i]);
                    paperArea.SetState(PaperArea.State.ScissorUsed);
                    break;
                case PaperArea.State.InMachine:
                    SetFlowers(paperArea, workshopParams[i]);
                    StartCoroutine(SendPaperToMachine(paperArea));
                    _wrappingMachine.StartMachine(paperArea);
                    break;
                case PaperArea.State.MachineDone:
                    SetFlowers(paperArea, workshopParams[i]);
                    StartCoroutine(SendPaperToMachine(paperArea));
                    paperArea.OnMachineDone();
                    _wrappingMachine.OpenMachine();
                    break;
                case PaperArea.State.InRibbonArea:
                    SetFlowers(paperArea, workshopParams[i]);
                    StartCoroutine(SendPaperToRibbon(paperArea));
                    break;
                case PaperArea.State.Done:
                    SetFlowers(paperArea, workshopParams[i]);
                    StartCoroutine(SendPaperToRibbon(paperArea));
                    paperArea.SetRibbonType(workshopParams[i].CurrentFlowers.RibbonType);
                    OnFlowerReady(workshopParams[i].CurrentFlowers, paperArea.gameObject);
                    break;
                default:
                    Debug.LogWarning($"#dukkan# Load: Unknown DukkanSaveState: {paperState}");
                    break;
            }
        }

        CheckCanAddFlowers();

    }

    private void SetFlowers(PaperArea paperArea, WorkshopParams workshopParams)
    {
        paperArea.gameObject.SetActive(true);
        paperArea.SetFlowers(workshopParams.CurrentFlowers);
        // TODO: set wrapping paper type
        // paperArea.SetPaperType(true);
    }

    private IEnumerator SendPaperToMachine(PaperArea paperArea)
    {
        var targetPosX = _trashBin.rect.width + _flowerTable.rect.width + _machineTable.rect.width / 2;
        targetPosX += _tableLayout.spacing * 2f;
        targetPosX -= Screen.width / 2f;
        _scrollTween?.Kill();

        yield return new WaitForEndOfFrame();

        _scrollContent.localPosition = new Vector3(-targetPosX, _scrollContent.localPosition.y, _scrollContent.localPosition.z);
        _wrappingMachine.TakeBouquet(paperArea);
    }

    private IEnumerator SendPaperToRibbon(PaperArea paperArea)
    {
        var targetPosX = _trashBin.rect.width + _flowerTable.rect.width + _machineTable.rect.width + _ribbonTableRect.rect.width / 2;
        targetPosX += _tableLayout.spacing * 2f;
        targetPosX -= Screen.width / 2f;
        _scrollTween?.Kill();

        yield return new WaitForEndOfFrame();

        _scrollTween = _scrollContent.DOLocalMoveX(-targetPosX, DURATION).SetEase(MACHINE_EASE);
        _ribbonTable.AddFlowerToRibbonArea(paperArea, _totalOrderCount, _unfinishedOrderCount - 1);
    }

    #endregion
}
