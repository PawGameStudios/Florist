using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    void Start()
    {
        SaveSystem.Inst.GeneralData.SetLife();
        References.MainPage.Open(new PageParams
        {
            PreviousPage = PageType.Bootstrapper
        });
    }
}
