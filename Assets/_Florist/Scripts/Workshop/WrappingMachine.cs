using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class WrappingMachine : MonoBehaviour
{
    public Rect Rect => _rectTransform.rect;
    [SerializeField] private Transform _paperSitPosition;
    [SerializeField] private Transform _door;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Image _progressImage;
    private Tween _progressTween, _openTween;

    private void OnDisable()
    {
        _progressTween?.Kill();
    }

    public void OpenMachine()
    {
        Debug.Log("Wrapping machine opened.");
        _progressTween?.Kill();
        _progressTween = _progressImage.DOFillAmount(0f, .1f).SetEase(Ease.Linear);

        _openTween?.Kill();
        _openTween = _door.DOLocalMoveX(800f, .5f).SetEase(Ease.InCubic);
    }

    public void CloseMachine(Action onComplete = null)
    {
        Debug.Log("Wrapping machine closed.");
        _openTween?.Kill();
        _openTween = _door.DOLocalMoveX(0f, .5f).SetEase(Ease.OutCubic).OnComplete(() => onComplete?.Invoke());
    }

    public void TakeBouquet(PaperArea paperArea)
    {
        paperArea.transform.SetPositionAndRotation(_paperSitPosition.position, _paperSitPosition.rotation);
    }

    public void StartMachine(PaperArea paperArea)
    {
        Debug.Log("Wrapping machine started.");

        CloseMachine(() =>
        {
            int level = SaveSystem.Inst.GeneralData.MachineLevel;
            float duration = Configs.WorkshopConfig.MachineInfo.CalculateDuration(level);
            _progressImage.fillAmount = 0f;

            _progressTween?.Kill();
            _progressTween = _progressImage.DOFillAmount(1f, duration).SetEase(Ease.Linear).OnComplete(() =>
            {
                HapticsController.PlayLightHaptic();
                paperArea.OnMachineDone();

                OpenMachine();
                Debug.Log("Wrapping machine process completed.");
            });
        });
    }
}
