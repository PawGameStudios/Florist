using UnityEngine;

public class MainUIController : MonoBehaviour
{
    [SerializeField] private Page _shopPage;

    public void OnPlayClicked()
    {
        Debug.Log("Play clicked");
    }

    public void OnShopClicked()
    {
        gameObject.SetActive(false);
        _shopPage.Open();
    }
}
