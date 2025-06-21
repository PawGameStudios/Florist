using Config;
using UnityEngine;
using UnityEngine.UI;

public class Flower : MonoBehaviour
{
    public Image FlowerImage;
    public FlowerType FlowerType => _flowerType;
    public FlowerColor FlowerColor => _flowerColor;

    [SerializeField] private FlowerType _flowerType;
    [SerializeField] private FlowerColor _flowerColor;
}
