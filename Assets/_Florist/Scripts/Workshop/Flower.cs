using Config;
using UnityEngine;

public class Flower : MonoBehaviour
{
    public FlowerType FlowerType => _flowerType;
    public FlowerColor FlowerColor => _flowerColor;

    [SerializeField] private FlowerType _flowerType;
    [SerializeField] private FlowerColor _flowerColor;
}
