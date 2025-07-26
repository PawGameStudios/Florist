using UnityEngine;

public class HandAnimController : MonoBehaviour
{
    [SerializeField] private Tutorial _tutorial;

    public void OnHandClicked()
    {
        _tutorial.OnHandAnimClicked();
    }
}
