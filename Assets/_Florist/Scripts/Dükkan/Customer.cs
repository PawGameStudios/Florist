using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Conversa.Runtime.Events;
using Febucci.UI.Core;
using Random = UnityEngine.Random;
using Conversa.Runtime;
using Config;
using AYellowpaper.SerializedCollections;
using System.Linq;
using MEC;

public class Customer : MonoBehaviour
{
    [Serializable]
    public struct FlowerDeliveredInfo
    {
        public Conversation Conversation;
        public HappinessState HappinessState;
        public float TipPercentage;
        public int HappinessChange;
        public List<OrderContentResult> OrderContentResults;
    }

    [Serializable]
    public struct OrderContentResult
    {
        public float OrderSimilarity;
        public bool IsRibbonCorrect;
        public bool IsWrappingPaperCorrect;
        public SerializedDictionary<FlowerType, int> ExtraFlowers;
        public SerializedDictionary<FlowerType, int> MissingFlowers;
    }

    public CustomerInfo CustomerInfo => _customerInfo;
    [SerializeField] private Transform _customerTransform;
    [SerializeField] private Transform _initialPositionRef;
    [SerializeField] private Transform _finalPositionRef;
    [SerializeField] private GameObject _speechBubbleObjects;
    [SerializeField] private Image _speechBubbleBgImage;
    [SerializeField] private TextMeshProUGUI _speechBubbleText;
    [SerializeField] private TextMeshProUGUI _speechBubbleTextForLineCount;
    [SerializeField] private List<Image> _speechBubbleButtonImages;
    [SerializeField] private List<Button> _speechBubbleButtons;
    [SerializeField] private List<TextMeshProUGUI> _speechBubbleButtonTexts;
    [SerializeField] private HorizontalLayoutGroup _speechBubbleButtonsLayoutGroup;
    [SerializeField] private Image _customerImage;
    [SerializeField] private TypewriterCore _typeWriter;
    [SerializeField] private List<BouquetModel> _bouquetsToOrder = new();
    private const float LINE_HEIGHT = 42;
    private const float ENTER_SCALE_Y = 1.06f;
    private const float IDLE_SCALE_Y = 1.015f;
    private const float IDLE_SCALE_X = .985f;
    private const float IDLE_DURATION = 1;
    private const int ORDER_SIMILARITY_LIMIT = 60;
    private Sequence _sequence, _idleSequence;
    private CustomerInfo _customerInfo;
    private float _waitingStartTime = 0;
    private Action _onTalkEnd;
    private bool _isWaitingForSpeechEnd = false;

    private void OnDisable()
    {
        _typeWriter.onTextShowed.RemoveAllListeners();
        _sequence?.Kill();
        _idleSequence?.Kill();
    }

    public void SetSpeechBubbleSprites()
    {
        Sprite bgSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.SpeechBubble);
        Sprite buttonSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.SpeechBubbleButton);

        // Set speech bubble sprites
        if (bgSprite != null)
            _speechBubbleBgImage.sprite = bgSprite;
        if (buttonSprite != null)
            foreach (var button in _speechBubbleButtonImages)
                button.sprite = buttonSprite;
    }

    public void SetCustomer(CustomerInfo customerInfo)
    {
        // Set customer info to UI
        _customerInfo = customerInfo;
        _customerImage.sprite = _customerInfo.Sprite;
        _speechBubbleObjects.SetActive(false);
        DetermineOrder();
    }

    public void PlayEnterAnimation(Action onComplete)
    {
        // Play enter animation
        _customerTransform.localPosition = _initialPositionRef.localPosition;
        gameObject.SetActive(true);

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(_customerTransform.DOLocalMoveY(_initialPositionRef.localPosition.y, 0));
        _sequence.Append(_customerTransform.DOLocalMoveY(_finalPositionRef.localPosition.y, .5f));
        _sequence.Join(_customerTransform.DOScaleY(ENTER_SCALE_Y, .1f));
        _sequence.Append(_customerTransform.DOScaleY(1, .2f).SetEase(Ease.OutQuad));
        _sequence.Append(_customerTransform.DOScaleY(1, 0).OnComplete(() =>
        {
            onComplete?.Invoke();
            PlayIdleAnimation();
        }));
    }

    public void PlayExitAnimation(Action onComplete = null)
    {
        StopIdleAnimation();

        // Play exit animation
        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(_customerTransform.DOScaleY(ENTER_SCALE_Y, .1f));
        _sequence.Append(_customerTransform.DOScaleY(1, .2f).SetEase(Ease.OutQuad));
        _sequence.Join(_customerTransform.DOLocalMoveY(_initialPositionRef.localPosition.y, .5f));
        _sequence.Append(_customerTransform.DOScaleY(1, 0).OnComplete(() =>
        {
            onComplete?.Invoke();
        }));
    }

    public void Talk(string localizationKey, string defaultMessage, List<Option> answerOptions, List<StringParseOptions> parseOptions)
    {
        Debug.Log($"#customer# Talk called with localizationKey: {localizationKey} and defaultMessage: {defaultMessage}");
        _typeWriter.onTextShowed.RemoveListener(OnSpeechEnd);

        // set text
        string localizedMessage = LocalizationManager.GetLocalizedText(localizationKey);
        string message = string.IsNullOrEmpty(localizedMessage) ? defaultMessage : localizedMessage;
        List<int> indexes = message.AllIndexesOf("_");
        message = message.RemoveIndexIndicators("_");

        // parse text
        int orderIndex = 0;
        int argCount = 0;
        for (int i = 0; i < parseOptions.Count; i++)
        {
            if (parseOptions[i] == StringParseOptions.None)
                break;
            argCount++;
        }

        string[] parseArgs = new string[argCount];
        for (int i = 0; i < parseOptions.Count; i++)
        {
            if (parseOptions[i] == StringParseOptions.None)
                break;

            int bouquetIndex = indexes[orderIndex];
            if (parseOptions[i] == StringParseOptions.FlowerType)
            {
                foreach (var flower in _bouquetsToOrder[bouquetIndex].Flowers)
                {
                    parseArgs[i] = LocalizationManager.GetLocalizedText(flower.FlowerType.ToString().ToLower());
                }
            }
            else if (parseOptions[i] == StringParseOptions.FlowerCountAndType)
            {
                int count = _bouquetsToOrder[bouquetIndex].Flowers.Count;
                for (int k = 0; k < count; k++)
                {
                    var flower = _bouquetsToOrder[bouquetIndex].Flowers[k];
                    int flowerCount = flower.Count;
                    string type;
                    if (flowerCount > 1)
                        type = LocalizationManager.GetLocalizedText($"{flower.FlowerType.ToString().ToLower()}_s");
                    else
                        type = LocalizationManager.GetLocalizedText(flower.FlowerType.ToString().ToLower());

                    if (k == count - 1)
                        parseArgs[i] += $"{flowerCount} {type}";
                    else if (k == count - 2)
                        parseArgs[i] += $"{flowerCount} {type} {LocalizationManager.GetLocalizedText("and", LocalizationManager.TextType.LOWER)} ";
                    else
                        parseArgs[i] += $"{flowerCount} {type}, ";
                }
            }
            else if (parseOptions[i] == StringParseOptions.FlowerCountColorType)
            {
                int count = _bouquetsToOrder[bouquetIndex].Flowers.Count;
                for (int k = 0; k < count; k++)
                {
                    var flower = _bouquetsToOrder[bouquetIndex].Flowers[k];
                    FlowerColor flowerColor = flower.FlowerColor;
                    int flowerCount = flower.Count;
                    string type;
                    if (flowerCount > 1)
                        type = LocalizationManager.GetLocalizedText($"{flower.FlowerType.ToString().ToLower()}_s");
                    else
                        type = LocalizationManager.GetLocalizedText(flower.FlowerType.ToString().ToLower());

                    if (flowerColor != FlowerColor.None)
                    {
                        if (k == count - 1)
                            parseArgs[i] += $"{flowerCount} {LocalizationManager.GetLocalizedText(flowerColor.ToString().ToLower())} {type}";
                        else if (k == count - 2)
                            parseArgs[i] += $"{flowerCount} {LocalizationManager.GetLocalizedText(flowerColor.ToString().ToLower())} {type} {LocalizationManager.GetLocalizedText("and", LocalizationManager.TextType.LOWER)}";
                        else
                            parseArgs[i] += $"{flowerCount} {LocalizationManager.GetLocalizedText(flowerColor.ToString().ToLower())} {type}, ";
                    }
                    else
                    {
                        if (k == count - 1)
                            parseArgs[i] += $"{flowerCount} {type}";
                        else if (k == count - 2)
                            parseArgs[i] += $"{flowerCount} {type} {LocalizationManager.GetLocalizedText("and", LocalizationManager.TextType.LOWER)} ";
                        else
                            parseArgs[i] += $"{flowerCount} {type}, ";
                    }

                }
            }
            else if (parseOptions[i] == StringParseOptions.BouquetType)
            {
                parseArgs[i] = LocalizationManager.GetLocalizedText(_bouquetsToOrder[bouquetIndex].BouquetType.ToString().ToLower());
            }
            else if (parseOptions[i] == StringParseOptions.BouquetContent)
            {
                string recipeString = "";
                int flowerCountInBouqet = _bouquetsToOrder[bouquetIndex].Flowers.Count;
                for (int k = 0; k < flowerCountInBouqet; k++)
                {
                    var flower = _bouquetsToOrder[bouquetIndex].Flowers[k];
                    FlowerColor flowerColor = flower.FlowerColor;
                    int flowerCount = flower.Count;
                    string type;
                    if (flowerCount > 1)
                        type = LocalizationManager.GetLocalizedText($"{flower.FlowerType.ToString().ToLower()}_s");
                    else
                        type = LocalizationManager.GetLocalizedText(flower.FlowerType.ToString().ToLower());

                    if (flowerColor == FlowerColor.None)
                        recipeString += $"{flowerCount} {type}";
                    else
                        recipeString += $"{flowerCount} {LocalizationManager.GetLocalizedText(flowerColor.ToString().ToLower())} {type}";

                    if (k != flowerCountInBouqet - 1)
                        recipeString += ", ";
                }
                parseArgs[i] = recipeString;
            }
            else if (parseOptions[i] == StringParseOptions.WrappingPaper)
            {
                var paper = _bouquetsToOrder[bouquetIndex].WrappingPaperType;
                parseArgs[i] = $"{LocalizationManager.GetLocalizedText(paper.ToString().ToLower())}";
            }
            else if (parseOptions[i] == StringParseOptions.Ribbon)
            {
                var ribbon = _bouquetsToOrder[bouquetIndex].RibbonType;
                parseArgs[i] = $"{LocalizationManager.GetLocalizedText(ribbon.ToString().ToLower())}";
            }
            orderIndex++;
        }

        message = string.Format(message, parseArgs);
        _speechBubbleTextForLineCount.text = message;

        // set speech bubble size
        Timing.RunCoroutine(SetSpeechBubbleOptions(message, answerOptions).CancelWith(gameObject));
    }

    public void Talk(string localizationKey, string defaultMessage, Action onCompleted)
    {
        Debug.Log($"#customer# Talk called with localizationKey: {localizationKey} and defaultMessage: {defaultMessage}");

        _isWaitingForSpeechEnd = true;
        _typeWriter.onTextShowed.RemoveListener(OnSpeechEnd);

        // set text
        string localizedMessage = LocalizationManager.GetLocalizedText(localizationKey);
        string message = string.IsNullOrEmpty(localizedMessage) ? defaultMessage : localizedMessage;

        _speechBubbleTextForLineCount.text = message;

        _onTalkEnd = onCompleted;
        Timing.RunCoroutine(SetSpeechBubbleOptions(message, null).CancelWith(gameObject));
    }

    public void StopTalking()
    {
        Debug.LogError("StopTalking called");
        _speechBubbleObjects.SetActive(false);
        _speechBubbleText.text = string.Empty;
        _typeWriter.SkipTypewriter();
    }

    public float GetOrderPayment()
    {
        // calculate flower price
        float price = 0;

        // add price of each flower type
        foreach (var bouquet in _bouquetsToOrder)
        {
            foreach (var flowers in bouquet.Flowers)
            {
                price += flowers.Count * Configs.WorkshopConfig.GetFlowerPrice(flowers.FlowerType);
            }
            price += Configs.WorkshopConfig.GetRibbonPrice(bouquet.RibbonType);
            price += Configs.WorkshopConfig.GetWrappingPaperPrice(bouquet.WrappingPaperType);
        }

        Debug.Log($"Flower price: {price}");
        return price;
    }

    public void StartTimer()
    {
        _waitingStartTime = Time.time;
    }

    public FlowerDeliveredInfo GetOrderInfo(List<BouquetModel> bouquetModels)
    {
        List<OrderContentResult> orderContentResults = CheckOrderContent(bouquetModels);

        int happinessChange = 0;
        HappinessState happinessState = HappinessState.None;
        for (int i = 0; i < orderContentResults.Count; i++)
        {
            OrderContentResult result = orderContentResults[i];

            // TODO: similarity value is hardcoded, change later
            if (result.OrderSimilarity >= ORDER_SIMILARITY_LIMIT && result.IsRibbonCorrect && result.IsWrappingPaperCorrect)
            {
                happinessState |= HappinessState.SameOrder;
                happinessChange += _customerInfo.HappinessChange[HappinessState.SameOrder];
            }
            else
            {
                happinessState |= HappinessState.DifferentOrder;
                happinessChange += _customerInfo.HappinessChange[HappinessState.DifferentOrder];
            }

            if (result.ExtraFlowers.Count > 0)
            {
                happinessState |= HappinessState.MoreFlowers;
                happinessChange += _customerInfo.HappinessChange[HappinessState.MoreFlowers];
            }

            if (result.MissingFlowers.Count > 0)
            {
                happinessState |= HappinessState.MissingFlowers;
                happinessChange += _customerInfo.HappinessChange[HappinessState.MissingFlowers];
            }
        }

        float totalWaitTimeInSeconds = Time.time - _waitingStartTime;
        if (_customerInfo.AcceptableWaitTime > totalWaitTimeInSeconds)
        {
            happinessState |= HappinessState.WaitedLong;
            happinessChange += _customerInfo.HappinessChange[HappinessState.WaitedLong];
        }

        float tip = 0;
        if (happinessChange > _customerInfo.HappinessTipLimit)
        {
            tip = Random.Range(_customerInfo.TipPercentage.x, _customerInfo.TipPercentage.y);
        }

        return new FlowerDeliveredInfo()
        {
            Conversation = Configs.LevelConfig.GetGoodbyeConvo(_customerInfo, happinessState),
            TipPercentage = tip,
            HappinessChange = happinessChange,
            HappinessState = happinessState,
            OrderContentResults = orderContentResults,
        };
    }

    public void OnSkipClicked()
    {
        _typeWriter.SkipTypewriter();
    }

    public void OnSpeechEnd()
    {
        if (_isWaitingForSpeechEnd)
        {
            Debug.LogError("OnSpeechEnd called 22222");
            _onTalkEnd?.Invoke();
            _onTalkEnd = null;
            _typeWriter.onTextShowed.RemoveListener(OnSpeechEnd);
        }
    }

    private void PlayIdleAnimation()
    {
        _sequence?.Kill();
        _idleSequence?.Kill();
        _idleSequence = DOTween.Sequence();
        _idleSequence.Append(_customerTransform.DOScaleY(IDLE_SCALE_Y, IDLE_DURATION).SetEase(Ease.Linear));
        _idleSequence.Join(_customerTransform.DOScaleX(IDLE_SCALE_X, IDLE_DURATION).SetEase(Ease.Linear));
        _idleSequence.Append(_customerTransform.DOScaleY(1, IDLE_DURATION).SetEase(Ease.Linear));
        _idleSequence.Join(_customerTransform.DOScaleX(1, IDLE_DURATION).SetEase(Ease.Linear));
        _idleSequence.OnComplete(PlayIdleAnimation);
    }

    private void StopIdleAnimation()
    {
        _idleSequence?.Kill();
    }

    private void DetermineOrder()
    {
        _bouquetsToOrder ??= new List<BouquetModel>();
        _bouquetsToOrder.Clear();

        // Determine order
        List<int> orderIndices = new();
        bool choseRandomOrder = _customerInfo.ChoseOrderRandomly;
        if (choseRandomOrder)
        {
            int safe = 0;
            int orderCount = Random.Range(1, _customerInfo.MaxOrderCount + 1);
            while (orderCount > 0)
            {
                if (safe++ > 100)
                {
                    Debug.LogError("Infinite loop detected in DetermineOrder. Exiting loop.");
                    break;
                }

                int bouquetTypeIndex = Random.Range(0, _customerInfo.Orders.Count);
                if (!orderIndices.Contains(bouquetTypeIndex))
                {
                    orderIndices.Add(bouquetTypeIndex);
                    orderCount--;
                }
            }
        }
        else
        {
            orderIndices = Enumerable.Range(0, _customerInfo.Orders.Count).ToList();
        }

        int bouquetCount = orderIndices.Count;
        for (int k = 0; k < bouquetCount; k++)
        {
            Order order = _customerInfo.Orders[orderIndices[k]];

            BouquetType bouquetTypeToOrder = order.BouquetType;
            if (bouquetTypeToOrder == BouquetType.Custom)
            {
                BouquetFlowerInfo flowerInfo = new();
                var bouquetModel = new BouquetModel()
                {
                    BouquetType = bouquetTypeToOrder
                };
                int flowerCount = order.CustomFlowers.Count;
                for (int i = 0; i < flowerCount; i++)
                {
                    flowerInfo.FlowerType = order.CustomFlowers[i].FlowerType;
                    flowerInfo.Count = order.CustomFlowers[i].Count;
                    flowerInfo.FlowerColor = order.CustomFlowers[i].FlowerColor;
                    bouquetModel.AddNewFlowers(flowerInfo);
                }

                bouquetModel.RibbonType = order.RibbonType;
                bouquetModel.WrappingPaperType = order.WrappingPaperType;

                _bouquetsToOrder.Add(bouquetModel);
            }
            else
            {
                Recipe recipe = Configs.WorkshopConfig.BouquetRecipes[bouquetTypeToOrder];
                var bouquetModel = new BouquetModel()
                {
                    Flowers = recipe.Bouquet.Flowers,
                    BouquetType = bouquetTypeToOrder,
                    RibbonType = order.RibbonType,
                    WrappingPaperType = order.WrappingPaperType,
                };
                _bouquetsToOrder.Add(bouquetModel);
            }
        }
    }

    private List<OrderContentResult> CheckOrderContent(List<BouquetModel> receivedBouquetModels)
    {
        List<OrderContentResult> results = new();

        for (int k = 0; k < _bouquetsToOrder.Count; k++)
        {
            OrderContentResult result = new()
            {
                ExtraFlowers = new SerializedDictionary<FlowerType, int>(),
                MissingFlowers = new SerializedDictionary<FlowerType, int>(),
            };
            BouquetModel orderedBouqet = _bouquetsToOrder[k];
            int mostSimilarIndex = 0;
            float mostSimilarPercentage = 0;

            int flowerCountInOrder = orderedBouqet.Flowers.Count;
            float flowerContributionToSimilarity = 100f / flowerCountInOrder;

            for (int i = 0; i < receivedBouquetModels.Count; i++)
            {
                BouquetModel receivedBouqet = receivedBouquetModels[i];
                float orderSimilarityPercentage = 0;

                foreach (var orderedFlower in orderedBouqet.Flowers)
                {
                    FlowerType orderedFlowerType = orderedFlower.FlowerType;
                    FlowerColor orderedFlowerColor = orderedFlower.FlowerColor;
                    int orderedFlowerCount = orderedFlower.Count;

                    BouquetFlowerInfo receivedFlowers = receivedBouqet.GetFlowersWithType(orderedFlowerType, orderedFlowerColor);
                    if (receivedFlowers != null)
                    {
                        int receivedFlowerCount = receivedFlowers.Count;
                        orderSimilarityPercentage += flowerContributionToSimilarity / orderedFlowerCount * receivedFlowerCount;
                    }
                }

                if (orderSimilarityPercentage > mostSimilarPercentage)
                {
                    mostSimilarPercentage = orderSimilarityPercentage;
                    mostSimilarIndex = i;
                }
            }

            // check flower similarity
            foreach (var orderedFlower in orderedBouqet.Flowers)
            {
                FlowerType orderedFlowerType = orderedFlower.FlowerType;
                FlowerColor orderedFlowerColor = orderedFlower.FlowerColor;
                int orderedFlowerCount = orderedFlower.Count;

                BouquetFlowerInfo receivedFlowers = receivedBouquetModels[mostSimilarIndex].GetFlowersWithType(orderedFlowerType, orderedFlowerColor);
                if (receivedFlowers != null)
                {
                    int receivedFlowerCount = receivedFlowers.Count;
                    if (receivedFlowerCount > orderedFlowerCount)
                    {
                        result.ExtraFlowers.Add(orderedFlowerType, receivedFlowerCount - orderedFlowerCount);
                    }
                    else if (receivedFlowerCount < orderedFlowerCount)
                    {
                        result.MissingFlowers.Add(orderedFlowerType, orderedFlowerCount - receivedFlowerCount);
                    }
                }
                else
                {
                    result.MissingFlowers.Add(orderedFlowerType, orderedFlowerCount);
                }
            }

            // check ribbon similarity
            result.IsRibbonCorrect = orderedBouqet.RibbonType == receivedBouquetModels[mostSimilarIndex].RibbonType;

            // check wrapping paper similarity
            result.IsWrappingPaperCorrect = orderedBouqet.WrappingPaperType == receivedBouquetModels[mostSimilarIndex].WrappingPaperType;

            // add similarity percentage to result
            result.OrderSimilarity = mostSimilarPercentage;

            // add result to list
            results.Add(result);
        }

        return results;
    }

    private IEnumerator<float> SetSpeechBubbleOptions(string message, List<Option> answerOptions = null)
    {
        yield return Timing.WaitForOneFrame;

        _speechBubbleObjects.SetActive(true);
        _typeWriter.ShowText(message);
        _typeWriter.onTextShowed.AddListener(OnSpeechEnd);

        int lineCount = _speechBubbleTextForLineCount.textInfo.lineCount;
        float height = lineCount * LINE_HEIGHT + LINE_HEIGHT * 2f;
        _speechBubbleBgImage.rectTransform.sizeDelta = new Vector2(_speechBubbleBgImage.rectTransform.sizeDelta.x,
                                                                    height);

        var buttonsParentTransform = _speechBubbleButtonsLayoutGroup.transform;
        buttonsParentTransform.position = new Vector3(buttonsParentTransform.position.x,
                                            _speechBubbleBgImage.transform.position.y - height - 100,
                                            buttonsParentTransform.position.z);

        int answerCount = answerOptions == null ? 0 : answerOptions.Count;
        for (int i = 0; i < answerCount; i++)
        {
            _speechBubbleButtonImages[i].gameObject.SetActive(true);
            _speechBubbleButtonTexts[i].gameObject.SetActive(true);
            _speechBubbleButtonTexts[i].text = LocalizationManager.GetLocalizedText(answerOptions[i].Message);

            _speechBubbleButtons[i].onClick.RemoveAllListeners();
            Action action = answerOptions[i].Advance;
            _speechBubbleButtons[i].onClick.AddListener(() =>
            {
                action?.Invoke();
            });
        }
        for (int i = answerCount; i < _speechBubbleButtonImages.Count; i++)
        {
            _speechBubbleButtonImages[i].gameObject.SetActive(false);
            _speechBubbleButtonTexts[i].gameObject.SetActive(false);
        }
        _speechBubbleButtonsLayoutGroup.enabled = false;
        _speechBubbleButtonsLayoutGroup.enabled = true;
        yield return Timing.WaitForOneFrame;
    }
}
