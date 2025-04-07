using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using Config;

public class WorkshopPage : Page
{
    [SerializeField] private Transform _workshopPanel;
    public float duration;
    public Ease ease;

    public override void Close(Action onCompleted = null)
    {
        gameObject.SetActive(false);
    }

    public override void Open(Action onCompleted = null)
    {
        gameObject.SetActive(true);
        int screenWidth = Screen.width;
        _workshopPanel.DOLocalMoveX(screenWidth / 2f, 0);
        _workshopPanel.DOLocalMoveX(-screenWidth / 2f, duration).SetEase(ease);
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

    [Button("OpenPage")]
    public void OpenPage()
    {
        Open();
    }
}
