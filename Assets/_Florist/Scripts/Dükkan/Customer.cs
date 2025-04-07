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
        public OrderContentResult OrderContentResult;
    }

    [Serializable]
    public struct OrderContentResult
    {
        public float OrderSimilarity;
        public float OrderBeauty;
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

        // TODO: localization
        string[] parseArgs = new string[argCount];
        for (int i = 0; i < parseOptions.Count; i++)
        {
            if (parseOptions[i] == StringParseOptions.None)
                break;

            if (parseOptions[i] == StringParseOptions.Count)
            {
                foreach (var flower in _bouquetsToOrder[orderIndex].Flowers)
                {
                    parseArgs[i] = flower.Value.ToString();
                }
            }
            else if (parseOptions[i] == StringParseOptions.FlowerType)
            {
                foreach (var flower in _bouquetsToOrder[orderIndex].Flowers)
                {
                    parseArgs[i] = LocalizationManager.GetLocalizedText(flower.Key.ToString().ToLower());
                }
                orderIndex++;
            }
            else if (parseOptions[i] == StringParseOptions.FlowerCountAndType)
            {
                foreach (var flower in _bouquetsToOrder[orderIndex].Flowers)
                {
                    string type = LocalizationManager.GetLocalizedText(flower.Key.ToString().ToLower());
                    int count = flower.Value;
                    parseArgs[i] = $"{count} {type}";
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
                int j = 0;
                foreach (var flower in _bouquetsToOrder[orderIndex].Flowers)
                {
                    string flowerType = LocalizationManager.GetLocalizedText(flower.Key.ToString().ToLower());
                    int flowerCount = flower.Value;

                    recipeString += $"{flowerCount} {flowerType}";
                    if (j != flowerCountInBouqet - 1)
                        recipeString += ", ";

                    j++;
                }
                parseArgs[i] = recipeString;
                orderIndex++;
            }
            else if (parseOptions[i] == StringParseOptions.WrappingPaper)
            {
                parseArgs[i] = _bouquetsToOrder[orderIndex].BeautyPercentage.ToString("0.00");
                orderIndex++;
            }
            else if (parseOptions[i] == StringParseOptions.Ribbon)
            {
                parseArgs[i] = LocalizationManager.GetLocalizedText(_customerInfo.Name);
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
                price += flowers.Value * Configs.WorkshopConfig.GetFlowerPrice(flowers.Key);
            }
            price += Configs.WorkshopConfig.GetRibbonPrice(bouquet.RibbonType);
            price += Configs.WorkshopConfig.GetWrappingPaperPrice(bouquet.WrappingPaperType);
        }

        Debug.Log($"Flower price: {price}");
        return price;
    }

    public FlowerDeliveredInfo GetOrderInfo(List<BouquetModel> bouquetModels)
    {
        OrderContentResult orderContentResult = CheckOrderContent(bouquetModels);

        return new FlowerDeliveredInfo()
        {
            Conversation = _customerInfo.GoodbyeConversation,
            Tip = Random.Range(_customerInfo.TipPercentage.x, _customerInfo.TipPercentage.y),
            HappinessChange = 0,
            OrderContentResult = orderContentResult,
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
                var bouquetModel = new BouquetModel()
                {
                    BeautyPercentage = 0,
                    BouquetType = bouquetTypeToOrder
                };
                int flowerCount = _customerInfo.FlowerTypes.Count;
                for (int i = 0; i < flowerCount; i++)
                {
                    FlowerType flowerType = _customerInfo.FlowerTypes[i];
                    if (bouquetModel.Flowers.ContainsKey(flowerType))
                    {
                        bouquetModel.Flowers[flowerType]++;
                    }
                    else
                    {
                        bouquetModel.Flowers.Add(flowerType, 1);
                    }
                }

                _bouquetsToOrder.Add(bouquetModel);
            }
            else
            {
                Recipe recipe = Configs.WorkshopConfig.BouquetRecipes[bouquetTypeToOrder];
                var bouquetModel = new BouquetModel()
                {
                    Flowers = recipe.Bouquet.Flowers,
                    BeautyPercentage = 0,
                    BouquetType = bouquetTypeToOrder
                };
                _bouquetsToOrder.Add(bouquetModel);
            }
        }
    }

    private OrderContentResult CheckOrderContent(List<BouquetModel> receivedBouquetModels)
    {
        OrderContentResult result = new()
        {
            ExtraFlowers = new(),
            MissingFlowers = new()
        };
        float totalSimilarity = 0;
        float totalBeauty = 0;

        for (int k = 0; k < _bouquetsToOrder.Count; k++)
        {
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
                    FlowerType orderedFlowerType = orderedFlower.Key;
                    int orderedFlowerCount = orderedFlower.Value;

                    if (receivedBouqet.Flowers.ContainsKey(orderedFlowerType))
                    {
                        int receivedFlowerCount = receivedBouqet.Flowers[orderedFlowerType];
                        orderSimilarityPercentage += flowerContributionToSimilarity / orderedFlowerCount * receivedFlowerCount;
                    }
                }

                if (orderSimilarityPercentage > mostSimilarPercentage)
                {
                    mostSimilarPercentage = orderSimilarityPercentage;
                    mostSimilarIndex = i;
                }
            }

            totalSimilarity += mostSimilarPercentage;
            totalBeauty += receivedBouquetModels[mostSimilarIndex].BeautyPercentage;

            foreach (var orderedFlower in orderedBouqet.Flowers)
            {
                FlowerType orderedFlowerType = orderedFlower.Key;
                int orderedFlowerCount = orderedFlower.Value;

                if (receivedBouquetModels[mostSimilarIndex].Flowers.ContainsKey(orderedFlowerType))
                {
                    int receivedFlowerCount = receivedBouquetModels[mostSimilarIndex].Flowers[orderedFlowerType];
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
        }

        result.OrderSimilarity = totalSimilarity / _bouquetsToOrder.Count;
        result.OrderBeauty = totalBeauty / _bouquetsToOrder.Count;
        return result;
    }
}
