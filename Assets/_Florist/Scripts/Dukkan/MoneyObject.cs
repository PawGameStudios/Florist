using UnityEngine;
using UnityEngine.UI;

public class MoneyObject : MonoBehaviour
{
    [SerializeField] private Image _image;
    private int _value;

    public MoneyObject SetSprite(Sprite sprite)
    {
        _image.sprite = sprite;
        return this;
    }

    public MoneyObject SetRandomRotation()
    {
        _image.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));
        return this;
    }

    public MoneyObject SetValue(int value)
    {
        _value = value;
        return this;
    }

    void OnMouseDown()
    {
        References.PosController.OnMoneyObjectClicked(_value);
        Destroy(gameObject);
    }
}
