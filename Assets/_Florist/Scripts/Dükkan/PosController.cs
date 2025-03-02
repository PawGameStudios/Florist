using DG.Tweening;
using TMPro;
using UnityEngine;

public class PosController : MonoBehaviour
{
    [SerializeField] private Color _red;
    [SerializeField] private Color _green;
    [SerializeField] private TextMeshProUGUI _posText;
    private Tween _tween;

    private void OnDisable()
    {
        _tween?.Kill();
    }

    public void ResetPos()
    {
        _posText.text = "";
    }

    public void ReceivePayment(float payment)
    {
        _posText.color = payment < 0 ? _red : _green;
        _posText.text = $"${payment:0. ##}";
        _tween?.Kill();
        _tween = transform.DOPunchScale(Vector3.one * 0.15f, .7f, vibrato: 1, elasticity: 1);
        SaveSystem.Inst.GeneralData.ChangeMoney(payment);
    }
}
