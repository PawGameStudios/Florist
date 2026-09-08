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

    private enum HintType
    {
        None,
        PaperSelect,
        FlowerSelect,
        Scissor,
        Machine,
        RibbonTable,
        Ribbon
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
    [SerializeField] private List<Transform> _initialPaperRefs;
    [SerializeField] private PaperBox _paperBox;

    [Title("Ribbon Table")]
    [SerializeField] private RibbonTable _ribbonTable;
    [SerializeField] private RectTransform _ribbonTableRect;

    [Title("Machine")]
    [SerializeField] private RectTransform _machineTable;
    [SerializeField] private WrappingMachine _wrappingMachine;

    [Title("Misc")]
    [SerializeField] private RectTransform _scrollContent;
    [SerializeField] private RectTransform _tableContent;
    [SerializeField] private HorizontalLayoutGroup _tableLayout;
    [SerializeField] private RectTransform _trashBin;
    [SerializeField] private Transform _scissor;
    [SerializeField] private Transform _workshopPanel;
    [SerializeField] private Tutorial _tutorial;
    [SerializeField] private Transform _workshopPanelInitPosRef;
    [SerializeField] private Transform _scrollContentInitPosRef;

    [Title("Convo History")]
    [SerializeField] private ConvoHistory _convoHistoryPanel;

    [Title("Tutorial")]
    [SerializeField] private Sprite _paperBoxSprite;
    [SerializeField] private Sprite _flowerBoxSprite;
    [SerializeField] private Sprite _ribbonSprite;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private GameObject _gypsumPlaceHolder1, _gypsumPlaceHolder2;

    private HintType _currentHintType = HintType.None;
    private bool _isInitialized = false;
    private bool _isInputWaiting = false;
    private Flower _selectedFlowerPrefab;
    private Tween _scrollTween;
    private bool _canFlowersBeSelected = false;
    private bool _isTutorialAllowFlowerSelect = true;
    private int _unfinishedOrderCount = 0;
    private int _totalOrderCount = 0;
    private int _tutorialFlowerCount = 0;
    private PaperArea _tutPaperArea;
    private List<string> _convoHistory = new();
    private bool _isTutorialStarted = false;
    private bool _canPaperBeDragged = false;
    private readonly List<FlowerBox> _flowerBoxes = new();
    private readonly List<BouquetModel> _finishedBouquetModels = new();
    private const int TUT_MAX_FLOWER_COUNT = 2;
    private const float DURATION = .8f;
    private const Ease SCROLL_INIT_EASE = Ease.OutBack;
    private const Ease MACHINE_EASE = Ease.OutQuint;

    void OnEnable()
    {
        _scrollRect.onValueChanged.AddListener(OnWorkshopScrolled);
        for (int i = 0; i < _initialPaperRefs.Count; i++)
        {
            _paperAreas[i].transform.SetPositionAndRotation(_initialPaperRefs[i].position, _initialPaperRefs[i].rotation);
            _paperAreas[i].gameObject.SetActive(false);
            _paperAreas[i].Reset();
        }
    }

    void OnDisable()
    {
        _scrollRect.onValueChanged.RemoveListener(OnWorkshopScrolled);
        CancelInvoke();
        _scrollTween?.Kill();
        _tutorial.FinishTutorial();
    }

    public override void Close(PageParams pageData = null, Action onCompleted = null)
    {
        base.Close(pageData, onCompleted);

        _tutorial.FinishTutorial();
        _flowerHand.gameObject.SetActive(false);
        _workshopPanel.localPosition = _workshopPanelInitPosRef.localPosition;
        _scrollContent.localPosition = _scrollContentInitPosRef.localPosition;
    }

    public override void Open(PageParams pageData = null, Action onCompleted = null)
    {
        Debug.Log($"Open WorkshopPage, LoadFromSaveData: {pageData?.LoadFromSaveData}");
        base.Open(pageData, onCompleted);

        if (pageData != null && pageData.LoadFromSaveData)
        {
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
            if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
            {
                _trashBin.gameObject.SetActive(false);
                _scrollRect.enabled = false;
                _isTutorialAllowFlowerSelect = false;
            }
            else
            {
                _trashBin.gameObject.SetActive(true);
                _scrollRect.enabled = true;
                _isTutorialAllowFlowerSelect = true;
            }

            int screenWidth = Screen.width;
            _workshopPanel.localPosition = new Vector3(screenWidth / 2f + 400, _workshopPanel.localPosition.y, _workshopPanel.localPosition.z);
            _scrollTween?.Kill();
            _scrollTween = _workshopPanel.DOLocalMoveX(-screenWidth / 2f, DURATION).SetEase(SCROLL_INIT_EASE).OnComplete(() =>
            {
                _scrollContent.sizeDelta = new(_tableContent.rect.width, _scrollContent.rect.height);
                _wrappingMachine.OpenMachine();
                onCompleted?.Invoke();

                if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
                {
                    _tutorial.Init()
                            .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand, Tutorial.ObjectActivationOptions.PopUp, Tutorial.ObjectActivationOptions.Bg)
                            .SetClickableState(Tutorial.ClickableState.HighlightArea)
                            .PointTo(_paperBox.FirstPaperPos, Tutorial.PointDirection.Right)
                            .Highlight(_paperBoxSprite, _paperBox.transform)
                            .SetExplanation(LocalizationManager.GetLocalizedText("tut_select_paper"))
                            .SetClickCallback(OnPaperSelected)
                            .StartTutorial();
                }
                else
                {
                    PrepareHint(specificHintType: HintType.PaperSelect);
                }
            });

            SetAvailableFlowers();
            SetAvailablePapers();
            SetAvailableRibbons();
            _isInitialized = true;
            _finishedBouquetModels.Clear();
        }
    }

    public WorkshopPage SetOrderCount(int count)
    {
        _unfinishedOrderCount = count;
        _totalOrderCount = count;
        return this;
    }

    public WorkshopPage SetConvoHistory(List<string> convoHistory)
    {
        _convoHistory = convoHistory;
        return this;
    }

    public void OnBoxSelected(Flower flowerPrefab, int boxIndex)
    {
        if (!_canFlowersBeSelected)
        {
            return;
        }

        if (!_isTutorialAllowFlowerSelect)
        {
            return;
        }

        _isInputWaiting = true;
        _selectedFlowerPrefab = flowerPrefab;
        _flowerHand.gameObject.SetActive(true);
        _flowerHand.position = _flowerBoxes[boxIndex].transform.position + Vector3.up * 100f;
    }

    public void OnPaperSelected(Sprite paperSprite, Sprite rollSprite, Sprite closedSprite, WrappingPaperType paperType)
    {
        for (int i = 0; i < _paperAreas.Count; i++)
        {
            if (_paperAreas[i].IsEmpty)
            {
                HapticsController.PlayButtonHaptic();

                _paperAreas[i].gameObject.SetActive(true);
                _paperAreas[i].GetPaperToArea(paperSprite, rollSprite, closedSprite, paperType, () => _canFlowersBeSelected = true);

                PrepareHint(specificHintType: HintType.FlowerSelect);

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

                if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
                {
                    OnScissorUsed();
                }
                else
                {
                    StopHints();
                }

                break;
            }
        }
    }

    public Flower CreateNewFlower(Vector3 targetPos, Vector3 targetRotation, Transform parent)
    {
        if (!_isInputWaiting)
            return null;

        if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
        {
            _tutorialFlowerCount++;
            if (_tutorialFlowerCount == TUT_MAX_FLOWER_COUNT)
            {
                CancelInvoke(nameof(FlowerPlaceLooper));
                CancelInvoke(nameof(FlowerPlaceLooper2));
                OnFlowersPlaced();
            }
            else if (_tutorialFlowerCount > TUT_MAX_FLOWER_COUNT)
            {
                return null;
            }
        }

        HapticsController.PlayMediumHaptic();

        PrepareHint(specificHintType: HintType.Scissor);

        Debug.Log($"Creating new flower at position: {targetPos}, rotation: {targetRotation}");

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
        if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
        {
            return false;
        }

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

        StopHints();

        HapticsController.PlayLightHaptic();
        _wrappingMachine.TakeBouquet(paperArea);

        _scrollTween?.Kill();
        _scrollTween = _scrollContent.DOLocalMoveX(-targetPosX, DURATION).SetEase(MACHINE_EASE).OnComplete(() =>
        {
            if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
            {
                _tutPaperArea = paperArea;
                OnMachineIntro();
            }
            else
            {
                _wrappingMachine.StartMachine(paperArea);
            }
        });

        CheckCanAddFlowers();
    }

    public void OnFlowerGivenToTrash(PaperArea paperArea)
    {
        Debug.Log("Flower given to trash.");

        if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
        {
            return;
        }

        HapticsController.PlayMediumHaptic();

        paperArea.gameObject.SetActive(false);

        CheckCanAddFlowers();
        PrepareHint(specificHintType: HintType.PaperSelect);
    }

    public void OnFlowerGivenToRibbon(PaperArea paperArea)
    {
        StopHints();
        Debug.Log("Flower given to ribbon area.");
        _ribbonTable.AddFlowerToRibbonArea(paperArea, _totalOrderCount, _unfinishedOrderCount - 1);

        var targetPosX = _trashBin.rect.width + _flowerTable.rect.width + _machineTable.rect.width + _ribbonTableRect.rect.width / 2;
        targetPosX += _tableLayout.spacing * 2f;
        targetPosX -= Screen.width / 2f;

        _tutorial.FinishTutorial();

        _scrollTween?.Kill();
        _scrollTween = _scrollContent.DOLocalMoveX(-targetPosX, DURATION).SetEase(MACHINE_EASE).OnComplete(() =>
        {
            if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
            {
                OnRibbonIntro();
            }
            else if (_ribbonTable.CanSelectRibbon)
            {
                PrepareHint(specificHintType: HintType.Ribbon);
            }
        });
        HapticsController.PlayLightHaptic();
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

            Close();
            References.DukkanPage.OnFlowerReady(orderInfo, bouquetObject);
        }
        else
        {
            PrepareHint(specificHintType: HintType.PaperSelect);
        }
    }

    public void OnConvoHistoryClicked()
    {
        _convoHistoryPanel.SetConvoHistory(_convoHistory);
    }

    private void SetAvailableFlowers()
    {
        for (int i = 0; i < _flowerBoxes.Count; i++)
        {
            Destroy(_flowerBoxes[i].gameObject);
        }
        _flowerBoxes.Clear();

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

        if (!_flowerTableParamsByFlowerCount.TryGetValue(flowerInfos.Count, out TableParams tableParams))
        {
            // The original six positions/table settings stay intact; extra prefab columns hold the extended catalog.
            tableParams = _flowerTableParamsByFlowerCount[6];
            tableParams.Width += Mathf.Ceil((flowerInfos.Count - 6) / 2f) * 325f;
        }

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
        // if (_isInitialized)
        //     return;

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
            };
            workshopParams.Add(workshopParam);
        }
        return workshopParams;
    }

    private void Load(List<WorkshopParams> workshopParams)
    {
        Debug.Log("Load");

        _finishedBouquetModels.Clear();
        _wrappingMachine.OpenMachine();

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
                    paperArea.SetPaper(workshopParams[i].CurrentFlowers.WrappingPaperType);
                    break;
                case PaperArea.State.ScissorInUse:
                    SetFlowers(paperArea, workshopParams[i]);
                    paperArea.SetState(PaperArea.State.ScissorUsed);
                    paperArea.SetPaper(workshopParams[i].CurrentFlowers.WrappingPaperType);
                    break;
                case PaperArea.State.InMachine:
                    SetFlowers(paperArea, workshopParams[i]);
                    StartCoroutine(SendPaperToMachine(paperArea));
                    paperArea.SetPaper(workshopParams[i].CurrentFlowers.WrappingPaperType);
                    paperArea.SetClosedPaper(workshopParams[i].CurrentFlowers.WrappingPaperType);
                    _wrappingMachine.StartMachine(paperArea);
                    break;
                case PaperArea.State.MachineDone:
                    SetFlowers(paperArea, workshopParams[i]);
                    StartCoroutine(SendPaperToMachine(paperArea));
                    paperArea.SetClosedPaper(workshopParams[i].CurrentFlowers.WrappingPaperType);
                    paperArea.OnMachineDone();
                    _wrappingMachine.OpenMachine();
                    break;
                case PaperArea.State.InRibbonArea:
                    SetFlowers(paperArea, workshopParams[i]);
                    paperArea.SetClosedPaper(workshopParams[i].CurrentFlowers.WrappingPaperType);
                    StartCoroutine(SendPaperToRibbon(paperArea));
                    break;
                case PaperArea.State.Done:
                    SetFlowers(paperArea, workshopParams[i]);
                    StartCoroutine(SendPaperToRibbon(paperArea));
                    paperArea.SetClosedPaper(workshopParams[i].CurrentFlowers.WrappingPaperType);
                    paperArea.SetRibbon(workshopParams[i].CurrentFlowers.RibbonType);
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


    #region Tutorial
    public bool CanPaperBeDragged()
    {
        if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
        {
            return _canPaperBeDragged;
        }
        return true;
    }

    private void OnPaperSelected()
    {
        if (_isTutorialStarted)
        {
            return;
        }

        FirebaseController.Instance.SendCustomEvent($"tutorial_workshop_paper_selected");

        _isTutorialStarted = true;
        _tutorial.FinishTutorialStep();
        _paperBox.SelectTutorialPaper();
        Invoke(nameof(OnPaperSelectedHelper), 1.1f);
    }

    private void OnPaperSelectedHelper()
    {
        _tutorial.Init()
                .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand, Tutorial.ObjectActivationOptions.PopUp, Tutorial.ObjectActivationOptions.Bg)
                .SetClickableState(Tutorial.ClickableState.HighlightArea)
                .PointTo(_flowerBoxes[0].transform.position, Tutorial.PointDirection.Right)
                .Highlight(_flowerBoxSprite, _flowerBoxes[0].transform)
                .SetExplanation(LocalizationManager.GetLocalizedText("tut_flower_explanation"))
                .SetClickCallback(OnFlowerSelected)
                .StartTutorial();
    }

    private void OnFlowerSelected()
    {
        _gypsumPlaceHolder1.SetActive(false);
        _gypsumPlaceHolder2.SetActive(false);

        _isTutorialAllowFlowerSelect = true;
        _flowerBoxes[0].OnBoxSelected();
        _isTutorialAllowFlowerSelect = false;

        FirebaseController.Instance.SendCustomEvent($"tutorial_workshop_flower_selected");

        _tutorial.Init()
            .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand, Tutorial.ObjectActivationOptions.PopUp)
            .SetExplanation(LocalizationManager.GetLocalizedText("tut_flower_place"))
            .SetClickableState(Tutorial.ClickableState.All)
            .StartTutorial();

        FlowerPlaceLooper();
    }

    private void FlowerPlaceLooper()
    {
        CancelInvoke(nameof(FlowerPlaceLooper));
        CancelInvoke(nameof(FlowerPlaceLooper2));

        _gypsumPlaceHolder1.SetActive(false);
        _gypsumPlaceHolder2.SetActive(false);

        _tutorial.PointTo(_gypsumPlaceHolder2.transform.position + new Vector3(0, 100, 0), Tutorial.PointDirection.Right)
                .ResumeHandAnim()
                .SetHandCallback(() =>
                {
                    Debug.Log("handcallback for gypsum placeholder 2");
                    _gypsumPlaceHolder2.SetActive(true);
                    _tutorial.StopHandAnim();
                    Invoke(nameof(FlowerPlaceLooper2), 1f);
                });
    }

    private void FlowerPlaceLooper2()
    {
        _tutorial.PointTo(_gypsumPlaceHolder1.transform.position + new Vector3(0, 100, 0), Tutorial.PointDirection.Right)
                .ResumeHandAnim()
                .SetHandCallback(() =>
                {
                    Debug.Log("handcallback for gypsum placeholder 1");
                    _tutorial.StopHandAnim();
                    _gypsumPlaceHolder1.SetActive(true);
                    Invoke(nameof(FlowerPlaceLooper), 1f);
                });
    }

    private void OnFlowersPlaced()
    {
        _gypsumPlaceHolder1.SetActive(false);
        _gypsumPlaceHolder2.SetActive(false);

        FirebaseController.Instance.SendCustomEvent($"tutorial_workshop_flowers_placed");

        _tutorial.Init()
            .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand, Tutorial.ObjectActivationOptions.PopUp)
            .PointTo(_scissor.position - new Vector3(0, _scissor.GetComponent<RectTransform>().sizeDelta.x / 2f, 0), Tutorial.PointDirection.Right)
            .SetExplanation(LocalizationManager.GetLocalizedText("tut_use_scissor"))
            .StartTutorial();
    }

    private void OnScissorUsed()
    {
        FirebaseController.Instance.SendCustomEvent($"tutorial_workshop_scissor_used");

        _canPaperBeDragged = true;

        _tutorial.Init()
            .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand, Tutorial.ObjectActivationOptions.PopUp)
            .SwipeBetween(_paperAreas[0].transform.position, _machineTable.transform.position)
            .SetExplanation(LocalizationManager.GetLocalizedText("tut_place_machine"))
            .SetActivationDelay(1.5f)
            .StartTutorial();
    }

    private void OnMachineIntro()
    {
        FirebaseController.Instance.SendCustomEvent($"tutorial_workshop_machine_intro");

        _tutorial.Init()
            .SetObjectActivation(Tutorial.ObjectActivationOptions.PopUp)
            .SetClickCallback(OnMachineIntroFinished)
            .SetDelayedCallback(2f, OnMachineIntroFinished)
            .SetClickableState(Tutorial.ClickableState.HighlightArea)
            .SetExplanation(LocalizationManager.GetLocalizedText("tut_machine_intro"))
            .StartTutorial();
    }

    private void OnMachineIntroFinished()
    {
        FirebaseController.Instance.SendCustomEvent($"tutorial_workshop_machine_intro_finished");

        _wrappingMachine.StartMachine(_tutPaperArea);

        int machineLevel = SaveSystem.Inst.GeneralData.MachineLevel + Configs.ShopConfig.GetMachineLevelBonus(SaveSystem.Inst.ShopData);
        float delay = Configs.WorkshopConfig.MachineInfo.CalculateDuration(machineLevel) + .75f;

        _tutorial.Init()
            .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand, Tutorial.ObjectActivationOptions.PopUp)
            .SwipeBetween(_paperAreas[0].transform.position, _ribbonTable.transform.position)
            .SetExplanation(LocalizationManager.GetLocalizedText("tut_place_ribbon"))
            .SetActivationDelay(delay)
            .StartTutorial();
    }

    private void OnRibbonIntro()
    {
        Invoke(nameof(ExplainRibbon), .1f);
    }

    private void ExplainRibbon()
    {
        FirebaseController.Instance.SendCustomEvent($"tutorial_workshop_ribbon_intro");

        _tutorial.Init()
            .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand, Tutorial.ObjectActivationOptions.PopUp, Tutorial.ObjectActivationOptions.Bg)
            .SetClickableState(Tutorial.ClickableState.HighlightArea)
            .PointTo(_ribbonTable.FirstRibbonTransform.position, Tutorial.PointDirection.Right)
            .Highlight(_ribbonSprite, _ribbonTable.FirstRibbonTransform)
            .Highlight2(_paperBoxSprite, _paperAreas[0].transform)
            .SetExplanation(LocalizationManager.GetLocalizedText("tut_select_ribbon"))
            .SetClickCallback(OnTutorialFinished)
            .StartTutorial();
    }

    private void OnTutorialFinished()
    {
        FirebaseController.Instance.SendCustomEvent($"tutorial_workshop_finished");

        _tutorial.CloseTutorial();
        _tutorial.FinishTutorial();
        _ribbonTable.OnRibbonClicked(0);

        SaveSystem.Inst.SaveData.IsTutorialFinished = true;
    }
    #endregion


    #region Hints
    private void CancelHintInvokes()
    {
        CancelInvoke(nameof(ShowPaperSelectHint));
        CancelInvoke(nameof(ShowFlowerSelectHint));
        CancelInvoke(nameof(ShowScissorHint));
        CancelInvoke(nameof(ShowMachineHint));
        CancelInvoke(nameof(ShowRibbonTableHint));
        CancelInvoke(nameof(ShowRibbonHint));
    }

    private void StopHints()
    {
        CancelHintInvokes();
        _currentHintType = HintType.None;
        _tutorial.FinishTutorial();
    }

    private void OnWorkshopScrolled(Vector2 position)
    {
        _tutorial.RefreshHandPosition();
    }

    public void OnRibbonSelectionStarted()
    {
        StopHints();
    }

    public void OnBouquetCut()
    {
        PrepareHint(specificHintType: HintType.Machine);
    }

    public void OnBouquetMachineDone()
    {
        PrepareHint(specificHintType: HintType.RibbonTable);
    }

    public void OnUncutBouquetGivenToMachine()
    {
        if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
            return;
        StopHints();
        _currentHintType = HintType.Scissor;
        ShowScissorHint();
    }

    private void PrepareHint(bool isNext = false, HintType specificHintType = HintType.None)
    {
        if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
        {
            return;
        }

        _tutorial.FinishTutorial();
        CancelHintInvokes();
        float delay = Configs.WorkshopConfig.GetHintDelay(SaveSystem.Inst.GeneralData.CurrentDayIndex);

        if (isNext)
        {
            _currentHintType++;
        }
        if (specificHintType != HintType.None)
        {
            _currentHintType = specificHintType;
        }

        switch (_currentHintType)
        {
            case HintType.PaperSelect:
                Invoke(nameof(ShowPaperSelectHint), delay);
                break;
            case HintType.FlowerSelect:
                Invoke(nameof(ShowFlowerSelectHint), delay);
                break;
            case HintType.Scissor:
                Invoke(nameof(ShowScissorHint), delay);
                break;
            case HintType.Machine:
                Invoke(nameof(ShowMachineHint), delay);
                break;
            case HintType.RibbonTable:
                Invoke(nameof(ShowRibbonTableHint), delay);
                break;
            case HintType.Ribbon:
                Invoke(nameof(ShowRibbonHint), delay);
                break;
        }
    }

    private void ShowPaperSelectHint()
    {
        FirebaseController.Instance.SendCustomEvent($"hint_workshop_paper_select");
        _tutorial.Init()
                .SetHandCoordinateSpace(_scrollContent)
                .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand)
                .PointTo(_paperBox.FirstPaperPos, Tutorial.PointDirection.Right)
                .StartTutorial();
    }

    private void ShowFlowerSelectHint()
    {
        FirebaseController.Instance.SendCustomEvent($"hint_workshop_flower_select");
        _tutorial.Init()
                .SetHandCoordinateSpace(_scrollContent)
                .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand)
                .PointTo(_flowerBoxes[0].transform.position, Tutorial.PointDirection.Right)
                .StartTutorial();
    }

    private void ShowScissorHint()
    {
        FirebaseController.Instance.SendCustomEvent($"hint_workshop_scissor");
        _tutorial.Init()
                .SetHandCoordinateSpace(_scrollContent)
                .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand)
                .PointTo(_scissor.position - new Vector3(0, ((RectTransform)_scissor).sizeDelta.x / 2f, 0), Tutorial.PointDirection.Right)
                .StartTutorial();
    }

    private void ShowMachineHint()
    {
        PaperArea paper = GetHintPaper(PaperArea.State.ScissorUsed);
        if (paper == null) return;
        FirebaseController.Instance.SendCustomEvent($"hint_workshop_machine");
        _tutorial.Init()
                .SetHandCoordinateSpace(_scrollContent)
                .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand)
                .SwipeBetween(paper.transform.position, _machineTable.transform.position)
                .StartTutorial();
    }

    private void ShowRibbonTableHint()
    {
        PaperArea paper = GetHintPaper(PaperArea.State.MachineDone);
        if (paper == null) return;
        FirebaseController.Instance.SendCustomEvent($"hint_workshop_ribbon_table");
        _tutorial.Init()
                .SetHandCoordinateSpace(_scrollContent)
                .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand)
                .SwipeBetween(paper.transform.position, _ribbonTable.transform.position)
                .StartTutorial();
    }

    private void ShowRibbonHint()
    {
        if (!_ribbonTable.CanSelectRibbon) return;
        FirebaseController.Instance.SendCustomEvent($"hint_workshop_ribbon");
        _tutorial.Init()
                .SetHandCoordinateSpace(_scrollContent)
                .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand)
                .PointTo(_ribbonTable.FirstRibbonTransform.position, Tutorial.PointDirection.Right)
                .StartTutorial();
    }

    private PaperArea GetHintPaper(PaperArea.State state)
    {
        foreach (PaperArea paper in _paperAreas)
        {
            if (paper.gameObject.activeInHierarchy && paper.PaperState == state)
                return paper;
        }
        return null;
    }
    #endregion

}
