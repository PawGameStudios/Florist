using UnityEngine;

public class Popup : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    public void Open()
    {
        gameObject.SetActive(true);
        _animator.Play("Open");
    }

    public void Close()
    {
        _animator.Play("Close");
    }

    public void OnCloseAnimFinished()
    {
        gameObject.SetActive(false);
    }
}
