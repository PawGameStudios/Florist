using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    void Start()
    {
        SaveSystem.Inst.GeneralData.SetLife();

        if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
        {
            References.OpeningPage.Open(new PageParams
            {
                PreviousPage = PageType.Bootstrapper
            });
        }
        else
        {
            References.MainPage.Open(new PageParams
            {
                PreviousPage = PageType.Bootstrapper
            });
        }


    }
}
