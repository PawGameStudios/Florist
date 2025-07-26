using System.Collections.Generic;
using Config;
using UnityEngine;
using UnityEngine.UI;

public class PaperBox : MonoBehaviour
{
    public Vector3 FirstPaperPos => _paperImages[0].transform.position;
    [SerializeField] private List<Image> _paperImages;
    [SerializeField] private Sprite _lockSprite;
    private List<WrappingPaperInfo> _papers;

    public void Initialize(List<WrappingPaperInfo> papers)
    {
        _papers = papers;
        int i = 0;
        for (; i < papers.Count; i++)
        {
            if (i >= _paperImages.Count)
                break;

            _paperImages[i].sprite = papers[i].Sprite;
            _paperImages[i].gameObject.SetActive(true);
        }
        for (; i < _paperImages.Count; i++)
        {
            _paperImages[i].sprite = _lockSprite;
            _paperImages[i].gameObject.SetActive(true);
        }
    }

    public void SelectTutorialPaper()
    {
        var selectedPaper = _papers[0];
        References.WorkshopPage.OnPaperSelected(selectedPaper.PaperSprite, selectedPaper.PaperRollSprite, selectedPaper.WrappingPaperType);
    }

    public void OnPaperClicked(int index)
    {
        if (index >= _papers.Count)
            return;

        var selectedPaper = _papers[index];
        References.WorkshopPage.OnPaperSelected(selectedPaper.PaperSprite, selectedPaper.PaperRollSprite, selectedPaper.WrappingPaperType);
    }
}
