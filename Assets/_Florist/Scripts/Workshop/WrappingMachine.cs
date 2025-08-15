using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using System.Collections;
using MEC;
using System.Collections.Generic;

public class WrappingMachine : MonoBehaviour
{
    public Rect Rect => _rectTransform.rect;
    [SerializeField] private Canvas _doorCanvas;
    [SerializeField] private Transform _paperSitPosition;
    [SerializeField] private Transform _door;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Image _progressImage;
    [SerializeField] private GameObject _dotsStartObject, _dotsFinalObject;
    [SerializeField] private GameObject[] _dotObjects;
    private Tween _progressTween, _openTween;

    private void OnDisable()
    {
        _progressTween?.Kill();
    }

    public void OpenMachine(Action onComplete = null)
    {
        Debug.Log("Wrapping machine opened.");
        _progressTween?.Kill();
        _progressTween = _progressImage.DOFillAmount(0f, .1f).SetEase(Ease.Linear);

        _openTween?.Kill();
        _openTween = _door.DOLocalMoveX(800f, .5f).SetEase(Ease.InCubic).OnComplete(() =>
        {
            onComplete?.Invoke();
        });
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

        _doorCanvas.sortingOrder = 10;

        CloseMachine(() =>
        {
            int level = SaveSystem.Inst.GeneralData.MachineLevel;
            float duration = Configs.WorkshopConfig.MachineInfo.CalculateDuration(level);
            _progressImage.fillAmount = 0f;

            var handle = Timing.RunCoroutine(ShowDots(), tag: "WrappingMachineDots");

            _progressTween?.Kill();
            _progressTween = _progressImage.DOFillAmount(1f, duration).SetEase(Ease.Linear).OnComplete(() =>
            {
                HapticsController.PlayLightHaptic();
                paperArea.OnMachineDone();

                OpenMachine(() =>
                {
                    if (handle != null && handle.IsValid)
                        Timing.KillCoroutines(handle);

                    ResetDots();
                    _dotsStartObject.SetActive(false);
                    _dotsFinalObject.SetActive(false);
                    _doorCanvas.sortingOrder = 4;
                });
                Debug.Log("Wrapping machine process completed.");
            });
        });
    }

    private void ResetDots()
    {
        foreach (var dot in _dotObjects)
        {
            dot.SetActive(false);
        }
    }

    private IEnumerator<float> ShowDots()
    {
        ResetDots();

        _dotsStartObject.SetActive(true);
        _dotsFinalObject.SetActive(true);

        while (true)
        {
            for (int i = 0; i < _dotObjects.Length; i++)
            {
                _dotObjects[i].SetActive(true);
                yield return Timing.WaitForSeconds(.13f);
            }

            ResetDots();
            yield return Timing.WaitForSeconds(.13f);
        }
    }
}
