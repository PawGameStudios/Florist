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

public class Customer : MonoBehaviour
{
    [Serializable]
    public struct FlowerDeliveredInfo
    {
        public Conversation Conversation;
        public float Tip;
        public int HappinessChange;
        public List<OrderContentResult> OrderContentResults;
    }

    [Serializable]
    public struct OrderContentResult
    {
        public float OrderSimilarity;
        public float OrderBeauty;
        public bool IsRibbonCorrect;
        public bool IsWrappingPaperCorrect;
        public SerializedDictionary<FlowerType, int> ExtraFlowers;
        public SerializedDictionary<FlowerType, int> MissingFlowers;
    }

    public CustomerInfo CustomerInfo => _customerInfo;
    [SerializeField] private Transform _initialPositionRef;
    [SerializeField] private Transform _finalPositionRef;
    [SerializeField] private GameObject _speechBubbleObjects;
    [SerializeField] private Image _speechBubbleBgImage;
    [SerializeField] private TextMeshProUGUI _speechBubbleText;
    [SerializeField] private TextMeshProUGUI _speechBubbleTextForLineCount;
    [SerializeField] private List<Image> _speechBubbleButtonImages;
    [SerializeField] private List<Button> _speechBubbleButtons;
    [SerializeField] private List<TextMeshProUGUI> _speechBubbleButtonTexts;
    [SerializeField] private Image _customerImage;
    [SerializeField] private TypewriterCore _typeWriter;
    [SerializeField] private List<BouquetModel> _bouquetsToOrder = new();
    private const float LINE_HEIGHT = 42;
    private const float ENTER_SCALE_Y = 1.06f;
    private const float IDLE_SCALE_Y = 1.015f;
    private const float IDLE_SCALE_X = .985f;
    private const float IDLE_DURATION = 1;
    private Sequence _sequence, _idleSequence;
    private CustomerInfo _customerInfo;

    private void OnDisable()
    {
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
        transform.localPosition = _initialPositionRef.localPosition;
        gameObject.SetActive(true);

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(transform.DOLocalMoveY(_initialPositionRef.localPosition.y, 0));
        _sequence.Append(transform.DOLocalMoveY(_finalPositionRef.localPosition.y, .5f));
        _sequence.Join(transform.DOScaleY(ENTER_SCALE_Y, .1f));
        _sequence.Append(transform.DOScaleY(1, .2f).SetEase(Ease.OutQuad));
        _sequence.Append(transform.DOScaleY(1, 0).OnComplete(() =>
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
        _sequence.Append(transform.DOScaleY(ENTER_SCALE_Y, .1f));
        _sequence.Append(transform.DOScaleY(1, .2f).SetEase(Ease.OutQuad));
        _sequence.Join(transform.DOLocalMoveY(_initialPositionRef.localPosition.y, .5f));
        _sequence.Append(transform.DOScaleY(1, 0).OnComplete(() =>
        {
            onComplete?.Invoke();
        }));
    }

    public void Talk(string localizationKey, string defaultMessage, List<Option> answerOptions, List<StringParseOptions> parseOptions)
    {
        _speechBubbleObjects.SetActive(true);

        // set text
        string localizedMessage = LocalizationManager.GetLocalizedText(localizationKey);
        string message = string.IsNullOrEmpty(localizedMessage) ? defaultMessage : localizedMessage;

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

            if (parseOptions[i] == StringParseOptions.FlowerType)
            {
                foreach (var flower in _bouquetsToOrder[orderIndex].Flowers)
                {
                    parseArgs[i] = LocalizationManager.GetLocalizedText(flower.FlowerType.ToString().ToLower());
                }
                orderIndex++;
            }
            else if (parseOptions[i] == StringParseOptions.FlowerCountAndType)
            {
                for (int k = 0; k < _bouquetsToOrder[orderIndex].Flowers.Count; k++)
                {
                    var flower = _bouquetsToOrder[orderIndex].Flowers[k];
                    string type = LocalizationManager.GetLocalizedText(flower.FlowerType.ToString().ToLower());
                    int count = flower.Count;
                    parseArgs[i] = $"{count} {type}";
                }
                orderIndex++;
            }
            else if (parseOptions[i] == StringParseOptions.FlowerCountColorType)
            {
                for (int k = 0; k < _bouquetsToOrder[orderIndex].Flowers.Count; k++)
                {
                    var flower = _bouquetsToOrder[orderIndex].Flowers[k];
                    string type = LocalizationManager.GetLocalizedText(flower.FlowerType.ToString().ToLower());
                    int count = flower.Count;
                    FlowerColor flowerColor = flower.FlowerColor;
                    parseArgs[i] += $"{count} {LocalizationManager.GetLocalizedText(flowerColor.ToString().ToLower())} {type}";
                }
                orderIndex++;
            }
            else if (parseOptions[i] == StringParseOptions.BouquetType)
            {
                parseArgs[i] = LocalizationManager.GetLocalizedText(_bouquetsToOrder[orderIndex].BouquetType.ToString().ToLower());
                orderIndex++;
            }
            else if (parseOptions[i] == StringParseOptions.BouquetContent)
            {
                string recipeString = "";
                int flowerCountInBouqet = _bouquetsToOrder[orderIndex].Flowers.Count;
                for (int k = 0; k < flowerCountInBouqet; k++)
                {
                    var flower = _bouquetsToOrder[orderIndex].Flowers[k];
                    string flowerType = LocalizationManager.GetLocalizedText(flower.FlowerType.ToString().ToLower());
                    int flowerCount = flower.Count;
                    FlowerColor flowerColor = flower.FlowerColor;

                    if (flowerColor == FlowerColor.None)
                        recipeString += $"{flowerCount} {flowerType}";
                    else
                        recipeString += $"{flowerCount} {LocalizationManager.GetLocalizedText(flowerColor.ToString().ToLower())} {flowerType}";

                    if (k != flowerCountInBouqet - 1)
                        recipeString += ", ";
                }
                parseArgs[i] = recipeString;
                orderIndex++;
            }
            else if (parseOptions[i] == StringParseOptions.WrappingPaper)
            {
                orderIndex--;
                var paper = _bouquetsToOrder[orderIndex].WrappingPaperType;
                parseArgs[i] = $"{LocalizationManager.GetLocalizedText(paper.ToString().ToLower())}";
                orderIndex++;
            }
            else if (parseOptions[i] == StringParseOptions.Ribbon)
            {
                orderIndex--;
                var ribbon = _bouquetsToOrder[orderIndex].RibbonType;
                parseArgs[i] = $"{LocalizationManager.GetLocalizedText(ribbon.ToString().ToLower())}";
                orderIndex++;
            }
        }

        message = string.Format(message, parseArgs);
        _speechBubbleTextForLineCount.text = message;
        _speechBubbleText.text = message;

        // set speech bubble size
        int lineCount = _speechBubbleTextForLineCount.textInfo.lineCount;
        float height = lineCount * LINE_HEIGHT + LINE_HEIGHT * 1.5f;
        _speechBubbleBgImage.rectTransform.sizeDelta = new Vector2(_speechBubbleBgImage.rectTransform.sizeDelta.x,
                                                                    height);

        int answerCount = answerOptions.Count;
        for (int i = 0; i < answerCount; i++)
        {
            _speechBubbleButtonImages[i].gameObject.SetActive(true);
            _speechBubbleButtonImages[i].transform.position = new Vector3(_speechBubbleButtonImages[i].transform.position.x,
                                                                        _speechBubbleBgImage.transform.position.y - height - 30,
                                                                        _speechBubbleButtonImages[i].transform.position.z);
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
    }

    public void StopTalking()
    {
        _speechBubbleObjects.SetActive(false);
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

    public FlowerDeliveredInfo GetOrderInfo(List<BouquetModel> bouquetModels)
    {
        List<OrderContentResult> orderContentResults = CheckOrderContent(bouquetModels);

        return new FlowerDeliveredInfo()
        {
            Conversation = _customerInfo.GoodbyeConversation,
            Tip = Random.Range(_customerInfo.TipPercentage.x, _customerInfo.TipPercentage.y),
            HappinessChange = 0,
            OrderContentResults = orderContentResults,
        };
    }

    public void OnSkipClicked()
    {
        _typeWriter.SkipTypewriter();
    }

    private void PlayIdleAnimation()
    {
        _sequence?.Kill();
        _idleSequence?.Kill();
        _idleSequence = DOTween.Sequence();
        _idleSequence.Append(transform.DOScaleY(IDLE_SCALE_Y, IDLE_DURATION).SetEase(Ease.Linear));
        _idleSequence.Join(transform.DOScaleX(IDLE_SCALE_X, IDLE_DURATION).SetEase(Ease.Linear));
        _idleSequence.Append(transform.DOScaleY(1, IDLE_DURATION).SetEase(Ease.Linear));
        _idleSequence.Join(transform.DOScaleX(1, IDLE_DURATION).SetEase(Ease.Linear));
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
        int bouquetCount = _customerInfo.BouquetTypes.Count;
        for (int k = 0; k < bouquetCount; k++)
        {
            var bouquetTypeToOrder = _customerInfo.BouquetTypes[Random.Range(0, _customerInfo.BouquetTypes.Count)];
            if (bouquetTypeToOrder == BouquetType.Custom)
            {
                BouquetFlowerInfo flowerInfo = new();
                var bouquetModel = new BouquetModel()
                {
                    BouquetType = bouquetTypeToOrder
                };
                int flowerCount = _customerInfo.CustomFlowers.Count;
                for (int i = 0; i < flowerCount; i++)
                {
                    flowerInfo.FlowerType = _customerInfo.CustomFlowers[i].FlowerType;
                    flowerInfo.Count = _customerInfo.CustomFlowers[i].Count;
                    flowerInfo.FlowerColor = _customerInfo.CustomFlowers[i].FlowerColor;
                    bouquetModel.AddNewFlowers(flowerInfo);
                }

                _bouquetsToOrder.Add(bouquetModel);
            }
            else
            {
                Recipe recipe = Configs.WorkshopConfig.BouquetRecipes[bouquetTypeToOrder];
                var bouquetModel = new BouquetModel()
                {
                    Flowers = recipe.Bouquet.Flowers,
                    BouquetType = bouquetTypeToOrder
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
}
