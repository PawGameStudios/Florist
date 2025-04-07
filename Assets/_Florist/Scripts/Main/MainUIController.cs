using UnityEngine;

public class MainUIController : MonoBehaviour
{
    public void OnPlayClicked()
    {
        gameObject.SetActive(false);
        References.DukkanPage.Open();
    }

    public void OnShopClicked()
    {
        gameObject.SetActive(false);
        References.ShopPage.Open();
    }
}
