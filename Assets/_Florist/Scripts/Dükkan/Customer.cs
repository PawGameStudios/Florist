using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Conversa.Runtime.Events;
using Febucci.UI.Core;
using static LevelConfig;
using Random = UnityEngine.Random;
using Conversa.Runtime;

public class Customer : MonoBehaviour
{
    public struct FlowerDeliveredInfo
    {
        public Conversation Conversation;
        public float Tip;
        public int HappinessChange;
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
    private const float LINE_HEIGHT = 42;
    private const float ENTER_SCALE_Y = 1.06f;
    private const float IDLE_SCALE_Y = 1.015f;
    private const float IDLE_SCALE_X = .985f;
    private const float IDLE_DURATION = 1;
    private Sequence _sequence, _idleSequence;
    private CustomerInfo _customerInfo;
    private List<BouquetType> _bouquetsToOrder = new();
    private BouquetType _bouquetTypeToOrder;
    private List<FlowerCount> _order;

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
        _customerImage.sprite = _customerInfo.GetRandomSprite();
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

    public void Talk(string localizationKey, string defaultMessage, List<Option> answerOptions, List<StringPaseOptions> parseOptions)
    {
        _speechBubbleObjects.SetActive(true);

        // set text
        string localizedMessage = LocalizationManager.GetLocalizedText(localizationKey);
        string message = string.IsNullOrEmpty(localizedMessage) ? defaultMessage : localizedMessage;

        // parse text
        int orderIndex = 0, bouquetIndex = 0;
        int argCount = 0;
        for (int i = 0; i < parseOptions.Count; i++)
        {
            if (parseOptions[i] == StringPaseOptions.None)
                break;
            argCount++;
        }
        // TODO: localization
        string[] parseArgs = new string[argCount];
        for (int i = 0; i < parseOptions.Count; i++)
        {
            if (parseOptions[i] == StringPaseOptions.None)
                break;

            if (parseOptions[i] == StringPaseOptions.Count)
            {
                parseArgs[i] = _order[orderIndex].Count.ToString();
            }
            else if (parseOptions[i] == StringPaseOptions.FlowerType)
            {
                parseArgs[i] = _order[orderIndex].FlowerType.ToString();
                orderIndex++;
            }
            else if (parseOptions[i] == StringPaseOptions.FlowerCountAndType)
            {
                string type = _order[orderIndex].FlowerType.ToString();
                int count = _order[orderIndex].Count;
                // TODO: make localization adjustments
                parseArgs[i] = $"{count} {type}";
                orderIndex++;
            }
            else if (parseOptions[i] == StringPaseOptions.BouquetType)
            {
                // TODO: localization
                parseArgs[i] = _bouquetsToOrder[bouquetIndex].ToString();
                bouquetIndex++;
            }
            else if (parseOptions[i] == StringPaseOptions.BouquetContent)
            {
                Recipe recipe = Configs.LevelConfig.BouquetRecipes[_bouquetsToOrder[bouquetIndex]];
                string recipeString = "";
                for (int j = 0; j < recipe.Flowers.Count; j++)
                {
                    // TODO: localization
                    recipeString += $"{recipe.Flowers[j].Count} {recipe.Flowers[j].FlowerType}";
                    if (j != recipe.Flowers.Count - 1)
                        recipeString += ", ";
                }
                parseArgs[i] = recipeString;
                bouquetIndex++;
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
            // TODO: localization
            _speechBubbleButtonTexts[i].text = answerOptions[i].Message;

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
        // Pay for flower
        float profitPercentage = _customerInfo.GetProfitPercentage();

        // calculate flower cost
        float flowerCost = 0;

        // add cost of each flower type
        foreach (var orderPair in _order)
        {
            flowerCost += orderPair.Count * Configs.LevelConfig.GetFlowerPrice(orderPair.FlowerType);
        }
        Debug.Log($"Flower cost: {flowerCost}, profit percentage: {profitPercentage}");
        return flowerCost * (100 + profitPercentage) / 100f;
    }

    public FlowerDeliveredInfo GetOrderInfo(BouquetModel bouquetModel)
    {
        return new FlowerDeliveredInfo()
        {
            Conversation = _customerInfo.GoodbyeConversation,
            Tip = Random.Range(_customerInfo.TipPercentage.x, _customerInfo.TipPercentage.y),
            HappinessChange = 0
        };
        // return _customerInfo.GoodbyeConversation;
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
        _order ??= new List<FlowerCount>();
        _order.Clear();

        // Determine order
        int bouquetCount = _customerInfo.BouquetCount;
        for (int k = 0; k < bouquetCount; k++)
        {
            _bouquetTypeToOrder = _customerInfo.BouquetTypes[Random.Range(0, _customerInfo.BouquetTypes.Count)];
            if (_bouquetTypeToOrder == BouquetType.Custom)
            {
                int flowerCount = _customerInfo.FlowerTypes.Count;
                for (int i = 0; i < flowerCount; i++)
                {
                    FlowerType flowerType = _customerInfo.FlowerTypes[i];
                    int orderIndex = _order.FindIndex(x => x.FlowerType == flowerType);
                    if (orderIndex != -1)
                    {
                        _order[orderIndex].Count++;
                    }
                    else
                    {
                        _order.Add(new FlowerCount(flowerType, 1));
                    }
                }
            }
            else
            {
                Recipe recipe = Configs.LevelConfig.BouquetRecipes[_bouquetTypeToOrder];
                for (int i = 0; i < recipe.Flowers.Count; i++)
                {
                    FlowerType flowerType = recipe.Flowers[i].FlowerType;
                    int orderIndex = _order.FindIndex(x => x.FlowerType == flowerType);
                    if (orderIndex != -1)
                    {
                        _order[orderIndex].Count += recipe.Flowers[i].Count;
                    }
                    else
                    {
                        _order.Add(new FlowerCount(flowerType, recipe.Flowers[i].Count));
                    }
                }
            }

            _bouquetsToOrder.Add(_bouquetTypeToOrder);
        }

    }

    private bool CheckOrderContent(BouquetModel bouquetModel)
    {
        // Check if the delivered bouquet matches the order
        List<FlowerCount> deliveredFlowers = new();
        foreach (var flower in bouquetModel.Flowers)
        {
            int orderIndex = deliveredFlowers.FindIndex(x => x.FlowerType == flower.Key);
            if (orderIndex != -1)
            {
                deliveredFlowers[orderIndex].Count += flower.Value;
            }
            else
            {
                deliveredFlowers.Add(new FlowerCount(flower.Key, flower.Value));
            }
        }

        if (_bouquetsToOrder.Count != bouquetModel.Flowers.Count)
            return false;

        for (int i = 0; i < _bouquetsToOrder.Count; i++)
        {
            if (_bouquetsToOrder[i] == BouquetType.Custom)
            {
                if (deliveredFlowers.Count != _order.Count)
                    return false;

                for (int j = 0; j < deliveredFlowers.Count; j++)
                {
                    if (deliveredFlowers[j].Count != _order[j].Count)
                        return false;
                }
            }
            else
            {
                Recipe recipe = Configs.LevelConfig.BouquetRecipes[_bouquetsToOrder[i]];
                if (deliveredFlowers.Count != recipe.Flowers.Count)
                    return false;

                for (int j = 0; j < deliveredFlowers.Count; j++)
                {
                    if (deliveredFlowers[j].Count != recipe.Flowers[j].Count)
                        return false;
                }
            }
        }

        return true;
    }
}
