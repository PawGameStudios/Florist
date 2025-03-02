using UnityEngine;

public class MainUIController : MonoBehaviour
{
    [SerializeField] private GameObject _bg;
    [SerializeField] private ShopPage _shopPage;
    [SerializeField] private DukkanPage _dukkanPage;

    public void OnPlayClicked()
    {
        gameObject.SetActive(false);
        _bg.SetActive(false);
        _dukkanPage.Open();
    }

    public void OnShopClicked()
    {
        gameObject.SetActive(false);
        _shopPage.Open();
    }
}
