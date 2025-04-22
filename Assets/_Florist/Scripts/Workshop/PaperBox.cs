using System.Collections.Generic;
using Config;
using UnityEngine;
using UnityEngine.UI;

public class PaperBox : MonoBehaviour
{
    [SerializeField] private Transform _paperImageParent;
    [SerializeField] private Image _paperImagePrefab;
    [SerializeField] private List<Transform> _paperImageReferences;
    private List<WrappingPaperInfo> _papers;

    public void Initilize(List<WrappingPaperInfo> papers)
    {
        _papers = papers;
        for (int i = 0; i < papers.Count; i++)
        {
            if (i >= _paperImageReferences.Count)
                break;

            var paper = Instantiate(_paperImagePrefab, _paperImageParent);
            paper.sprite = papers[i].Sprite;
            paper.transform.SetPositionAndRotation(_paperImageReferences[i].position, _paperImageReferences[i].rotation);
            paper.transform.localScale = _paperImageReferences[i].localScale;
            paper.gameObject.SetActive(true);
        }
    }

    public void OnPaperClicked(int index)
    {
        if (index >= _papers.Count)
            return;

        var selectedPaper = _papers[index];
        References.WorkshopPage.OnPaperSelected(selectedPaper.PaperOpenAnimation);
    }
}
